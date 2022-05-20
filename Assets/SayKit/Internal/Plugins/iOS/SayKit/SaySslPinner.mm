#import "SaySslPinner.h"

#import <CommonCrypto/CommonDigest.h>

#define SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(v)  ([[[UIDevice currentDevice] systemVersion] compare:(v) options:NSNumericSearch] != NSOrderedAscending)

NSString* const kSSPPublicKeyHashes = @"PublicKeyHashes";
NSString* const kSSPEnforcePinning = @"EnforcePinning";
NSString* const kSSPExcludeSubdomainFromParentPolicy = @"ExcludeSubdomainFromParentPolicy";
NSString* const kSSPIncludeSubdomains = @"IncludeSubdomains";
NSString* const kSSPPublicKeyAlgorithms = @"PublicKeyAlgorithms";
NSString* const kSSPExpirationDate = @"ExpirationDate";

static constexpr int ShouldAllowConnection = 0;
static constexpr int ShouldBlockConnection = 1;
static constexpr int DomainNotPinned = 2;

static constexpr int TrustEvaluationSuccess = 0;
static constexpr int TrustEvaluationFailedNoMatchingPin = 1;
static constexpr int TrustEvaluationErrorInvalidParameters = 2;
static constexpr int TrustEvaluationFailedInvalidCertificateChain = 3;
static constexpr int TrustEvaluationErrorCouldNotGenerateSpkiHash = 4;

static const size_t kMaxHostnameLen = 255;
static const char kUpperLowerDistance = 'A' - 'a';

@interface SSPSPKIHashCache : NSObject
- (instancetype)init;
- (NSData * _Nullable)hashSubjectPublicKeyInfoFromCertificate:(SecCertificateRef)certificate;
@end

typedef NSDictionary<NSString*, id> DomainPinningPolicy;

@interface SaySslPinner ()
//@property (nonatomic) SSPSPKIHashCache *spkiHashCache;
@property (nonatomic, readonly, nonnull) NSDictionary<NSString *, DomainPinningPolicy *> *domainPinningPolicies;
@end

static int IsStringASCII(const char* s) {
  const char* it = s;
  for (; *it != 0; ++it) {
    unsigned const char unsigned_char = (unsigned char)*it;
    if (unsigned_char > 0x7f) {
      return 0;
    }
  }
  return 1;
}

static __inline__ void ToLowerASCII(char* buf, const char* end) {
  for (; buf < end; ++buf) {
    char c = *buf;
    if (c >= 'A' && c <= 'Z') {
      *buf = c - kUpperLowerDistance;
    }
  }
}

static int IsValidHostname(const char* hostname) {
  /*
   * http://www.ietf.org/rfc/rfc1035.txt (DNS) and
   * http://tools.ietf.org/html/rfc1123 (Internet host requirements)
   * specify a maximum hostname length of 255 characters. To make sure
   * string comparisons, etc are bounded elsewhere in the codebase, we
   * enforce the 255 character limit here. There are various other
   * hostname constraints specified in the RFCs (63 bytes per
   * hostname-part, etc) but we do not enforce those here since doing
   * so would not change correctness of the overall implementation,
   * and it's possible that hostnames used in other contexts
   * (e.g. outside of DNS) would not be subject to the 63-byte
   * hostname-part limit. So we let the DNS layer enforce its policy,
   * and enforce only the maximum hostname length here.
   */
  if (strnlen(hostname, kMaxHostnameLen + 1) > kMaxHostnameLen) {
    return 0;
  }

  /*
   * All hostnames must contain only ASCII characters. If a hostname
   * is passed in that contains non-ASCII (e.g. an IDN that hasn't been
   * converted to ASCII via punycode) we want to reject it outright.
   */
  if (IsStringASCII(hostname) == 0) {
    return 0;
  }

  return 1;
}

static __inline__ void ReplaceChar(char* value, char old, char newval) {
  while ((value = strchr(value, old)) != NULL) {
    *value = newval;
    ++value;
  }
}

/*
 * Iterates the hostname-parts between start and end in reverse order,
 * separated by the character specified by sep. For instance if the
 * string between start and end is "foo\0bar\0com" and sep is the null
 * character, we will return a pointer to "com", then "bar", then
 * "foo".
 */
static const char* GetNextHostnamePartImpl(const char* start,
                                           const char* end,
                                           char sep,
                                           void** ctx) {
  const char* last;
  const char* i;

  if (*ctx == NULL) {
    *ctx = (void*) end;

    /*
     * Special case: a single trailing dot indicates a fully-qualified
     * domain name. Skip over it.
     */
    if (end > start && *(end - 1) == sep) {
      *ctx = (void*) (end - 1);
    }
  }
  last = (const char*)*ctx;
  if (start > last) return NULL;
  for (i = last - 1; i >= start; --i) {
    if (*i == sep) {
      *ctx = (void*) i;
      return i + 1;
    }
  }
  if (last != start && *start != 0) {
    /*
     * Special case: If we didn't find a match, but the context
     * indicates that we haven't visited the first component yet, and
     * there is a non-NULL first component, then visit the first
     * component.
     */
    *ctx = (void*) start;
    return start;
  }
  return NULL;
}

static __inline__ int IsWildcardComponent(const char* component) {
  if (component[0] == '*') {
    return 1;
  }
  return 0;
}

static __inline__ int IsExceptionComponent(const char* component) {
  if (component[0] == '!') {
    return 1;
  }
  return 0;
}

static __inline__ int IsInvalidComponent(const char* component) {
  if (component == NULL ||
      component[0] == 0 ||
      IsExceptionComponent(component) ||
      IsWildcardComponent(component)) {
    return 1;
  }
  return 0;
}

static const char* GetNextHostnamePart(const char* start,
                                       const char* end,
                                       char sep,
                                       void** ctx) {
  const char* hostname_part = GetNextHostnamePartImpl(start, end, sep, ctx);
  if (IsInvalidComponent(hostname_part)) {
    return NULL;
  }
  return hostname_part;
}

static size_t GetRegistryLengthImpl(
    const char* value,
    const char* value_end,
    const char sep) {
  const char* registry;
  size_t match_len;

  while (*value == sep && value < value_end) {
    /* Skip over leading separators. */
    ++value;
  }

  void* ctx = NULL;
  const char* root_hostname_part = GetNextHostnamePart(value, value_end, sep, &ctx);
  /*
   * See if the root hostname-part is in the table. If it's not in
   * the table, then consider the unknown registry to be a valid
   * registry.
   */
  if (root_hostname_part != NULL) {
    registry = root_hostname_part;
  }
  if (registry == NULL) {
    return 0;
  }
    
  if (registry < value || registry >= value_end) {
    /* Error cases. */
    assert(registry >= value);
    assert(registry < value_end);
    return 0;
  }
  match_len = (size_t) (value_end - registry);
  return match_len;
}

static size_t GetRegistryLength(const char* hostname) {
  const char* buf_end;
  char* buf;
  size_t registry_length;

  if (hostname == NULL) {
    return 0;
  }
  if (IsValidHostname(hostname) == 0) {
    return 0;
  }

  /*
   * Replace dots between hostname parts with the null byte. This
   * allows us to index directly into the string and refer to each
   * hostname-part as if it were its own null-terminated string.
   */
  buf = strdup(hostname);
  if (buf == NULL) {
    return 0;
  }
  ReplaceChar(buf, '.', '\0');

  buf_end = buf + strlen(hostname);
  assert(*buf_end == 0);

  /* Normalize the input by converting all characters to lowercase. */
  ToLowerASCII(buf, buf_end);
  registry_length = GetRegistryLengthImpl(buf, buf_end, '\0');
  free(buf);
  return registry_length;
}

static NSUInteger isSubdomain(NSString *domain, NSString *subdomain)
{
    // Ensure that the TLDs are the same; this can get tricky with TLDs like .co.uk so we take a cautious approach
    size_t domainRegistryLength = GetRegistryLength([domain UTF8String]);
    size_t subdomainRegistryLength = GetRegistryLength([subdomain UTF8String]);
    if (subdomainRegistryLength != domainRegistryLength)
    {
        return 0;
    }
    NSString *domainTld = [domain substringFromIndex: [domain length] - domainRegistryLength];
    NSString *subdomainTld = [subdomain substringFromIndex: [subdomain length] - subdomainRegistryLength];
    if (![domainTld isEqualToString:subdomainTld])
    {
        return 0;
    }
    
    // Retrieve the main domain without the TLD but append a . at the beginning
    // When initializing SaySslPinner, we check that [domain length] > domainRegistryLength
    NSString *domainLabel = [@"." stringByAppendingString:[domain substringToIndex:([domain length] - domainRegistryLength - 1)]];
    
    // Retrieve the subdomain's domain without the TLD
    NSString *subdomainLabel = [subdomain substringToIndex:([subdomain length] - domainRegistryLength - 1)];
    
    // Does the subdomain contain the domain
    NSArray *subComponents = [subdomainLabel componentsSeparatedByString:domainLabel];
    if ([[subComponents lastObject] isEqualToString:@""])
    {
        // This is a subdomain
        return [domainLabel length];
    }
    return 0;
}

static NSString * _Nullable getPinningConfigurationKeyForDomain(NSString * _Nonnull hostname , NSDictionary<NSString *, DomainPinningPolicy *> * _Nonnull domainPinningPolicies)
{
    NSString *notedHostname = nil;
    if (domainPinningPolicies[hostname] == nil)
    {
        NSUInteger bestMatch = 0;

        // No pins explicitly configured for this domain
        // Look for an includeSubdomain pin that applies
        for (NSString *pinnedServerName in domainPinningPolicies)
        {
            // Check each domain configured with the includeSubdomain flag
            if ([domainPinningPolicies[pinnedServerName][kSSPIncludeSubdomains] boolValue])
            {
                // Is the server a subdomain of this pinned server?
                NSUInteger currentMatch = isSubdomain(pinnedServerName, hostname);
                if (currentMatch > 0 && currentMatch > bestMatch)
                {
                    // Yes; let's use the parent domain's pinning configuration
                    bestMatch = currentMatch;
                    notedHostname = pinnedServerName;
                }
            }
        }
    }
    else
    {
        // This hostname has a pinnning configuration
        notedHostname = hostname;
    }
    return notedHostname;
}

static SSPSPKIHashCache *hashCache = [[SSPSPKIHashCache alloc] init];

static int verifyPublicKeyPin(SecTrustRef serverTrust, NSString *serverHostname, NSSet<NSData *> *knownPins)
{
    NSCParameterAssert(serverTrust);
    NSCParameterAssert(knownPins);
    if ((serverTrust == NULL) || (knownPins == nil))
    {
        NSLog(@"Invalid pinning parameters for %@", serverHostname);
        return TrustEvaluationErrorInvalidParameters;
    }

    // First re-check the certificate chain using the default SSL validation in case it was disabled
    // This gives us revocation (only for EV certs I think?) and also ensures the certificate chain is sane
    // And also gives us the exact path that successfully validated the chain
    CFRetain(serverTrust);
    
    // Create and use a sane SSL policy to force hostname validation, even if the supplied trust has a bad
    // policy configured (such as one from SecPolicyCreateBasicX509())
    SecPolicyRef SslPolicy = SecPolicyCreateSSL(YES, (__bridge CFStringRef)(serverHostname));
    SecTrustSetPolicies(serverTrust, SslPolicy);
    CFRelease(SslPolicy);
    
    SecTrustResultType trustResult = kSecTrustResultInvalid;
    if (SecTrustEvaluate(serverTrust, &trustResult) != errSecSuccess)
    {
        NSLog(@"SecTrustEvaluate error for %@", serverHostname);
        CFRelease(serverTrust);
        return TrustEvaluationErrorInvalidParameters;
    }
    
    if ((trustResult != kSecTrustResultUnspecified) && (trustResult != kSecTrustResultProceed))
    {
        // Default SSL validation failed
        CFDictionaryRef evaluationDetails = SecTrustCopyResult(serverTrust);
        NSLog(@"Error: default SSL validation failed for %@: %@", serverHostname, evaluationDetails);
        CFRelease(evaluationDetails);
        CFRelease(serverTrust);
        return TrustEvaluationFailedInvalidCertificateChain;
    }
    
    // Check each certificate in the server's certificate chain (the trust object); start with the CA all the way down to the leaf
    CFIndex certificateChainLen = SecTrustGetCertificateCount(serverTrust);
    for(int i=(int)certificateChainLen-1;i>=0;i--)
    {
        // Extract the certificate
        SecCertificateRef certificate = SecTrustGetCertificateAtIndex(serverTrust, i);
        CFStringRef certificateSubject = SecCertificateCopySubjectSummary(certificate);
        CFRelease(certificateSubject);
        
        // Generate the subject public key info hash
        NSData *subjectPublicKeyInfoHash = [hashCache hashSubjectPublicKeyInfoFromCertificate:certificate];
        if (subjectPublicKeyInfoHash == nil)
        {
            NSLog(@"Error - could not generate the SPKI hash for %@", serverHostname);
            CFRelease(serverTrust);
            return TrustEvaluationErrorCouldNotGenerateSpkiHash;
        }
        
        // Is the generated hash in our set of pinned hashes ?
        if ([knownPins containsObject:subjectPublicKeyInfoHash])
        {
            CFRelease(serverTrust);
            return TrustEvaluationSuccess;
        }
    }
    
    // If we get here, we didn't find any matching SPKI hash in the chain
    CFRelease(serverTrust);
    return TrustEvaluationFailedNoMatchingPin;
}

@implementation SaySslPinner

-(instancetype)initWithConfig:(NSDictionary*)config;
{
    if (self = [super init])
    {
        _domainPinningPolicies = [NSDictionary dictionaryWithDictionary:config];
    }
    return self;
}

- (BOOL)handleChallenge:(NSURLAuthenticationChallenge * _Nonnull)challenge
      completionHandler:
        (void (^ _Nonnull)(NSURLSessionAuthChallengeDisposition disposition, NSURLCredential * _Nullable credential))completionHandler
{
    BOOL wasChallengeHandled = NO;
    //if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"10.0"))
    {
        if ([challenge.protectionSpace.authenticationMethod isEqualToString:NSURLAuthenticationMethodServerTrust])
        {
            // Check the trust object against the pinning policy
            SecTrustRef serverTrust = challenge.protectionSpace.serverTrust;
            NSString *serverHostname = challenge.protectionSpace.host;
            
            int trustDecision = [self evaluateTrust:serverTrust forHostname:serverHostname];
            if (trustDecision == ShouldAllowConnection)
            {
                // Success
                wasChallengeHandled = YES;
                completionHandler(NSURLSessionAuthChallengeUseCredential, [NSURLCredential credentialForTrust:serverTrust]);
            }
            else if (trustDecision == DomainNotPinned)
            {
                // Domain was not pinned; we need to do the default validation to avoid disabling SSL validation for all non-pinned domains
                wasChallengeHandled = YES;
                completionHandler(NSURLSessionAuthChallengePerformDefaultHandling, NULL);
            }
            else
            {
                // Pinning validation failed - block the connection
                wasChallengeHandled = YES;
                completionHandler(NSURLSessionAuthChallengeCancelAuthenticationChallenge, NULL);
            }
        }
    }
    return wasChallengeHandled;
}

- (int)evaluateTrust:(SecTrustRef _Nonnull)serverTrust forHostname:(NSString * _Nonnull)serverHostname
{
    int finalTrustDecision = ShouldBlockConnection;
        
    if ((serverTrust == NULL) || (serverHostname == nil))
    {
        NSLog(@"Pin validation error - invalid parameters for %@", serverHostname);
        return finalTrustDecision;
    }
    CFRetain(serverTrust);
    
    // Retrieve the pinning configuration for this specific domain, if there is one
    NSString *domainConfigKey = getPinningConfigurationKeyForDomain(serverHostname, self.domainPinningPolicies);
    if (domainConfigKey == nil)
    {
        // The domain has no pinning policy: nothing to do/validate
        finalTrustDecision = DomainNotPinned;
    }
    else
    {
        // This domain has a pinning policy
        NSDictionary *domainConfig = self.domainPinningPolicies[domainConfigKey];
        
        // Has the pinning policy expired?
        NSDate *expirationDate = domainConfig[kSSPExpirationDate];
        if (expirationDate != nil && [expirationDate compare:[NSDate date]] == NSOrderedAscending)
        {
            // Yes the policy has expired
            finalTrustDecision = DomainNotPinned;
        }
        else if ([domainConfig[kSSPExcludeSubdomainFromParentPolicy] boolValue])
        {
            // This is a subdomain that was explicitly excluded from the parent domain's policy
            finalTrustDecision = DomainNotPinned;
        }
        else
        {
            // The domain has a pinning policy that has not expired
            // Look for one the configured public key pins in the server's evaluated certificate chain
            int validationResult = verifyPublicKeyPin(serverTrust,
                                                      serverHostname,
                                                      domainConfig[kSSPPublicKeyHashes]);
            
            if (validationResult == TrustEvaluationSuccess)
            {
                // Pin validation was successful
                finalTrustDecision = ShouldAllowConnection;
            }
            else
            {
                // Pin validation failed
                NSLog(@"Pin validation failed for %@", serverHostname);
                
                if (validationResult == TrustEvaluationFailedNoMatchingPin)
                {
                    // Is pinning enforced?
                    if ([domainConfig[kSSPEnforcePinning] boolValue] == YES)
                    {
                        // Yes - Block the connection
                        finalTrustDecision = ShouldBlockConnection;
                    }
                    else
                    {
                        finalTrustDecision = ShouldAllowConnection;
                    }
                }
                else
                {
                    // Misc pinning errors (such as invalid certificate chain) - block the connection
                    finalTrustDecision = ShouldBlockConnection;
                }
            }
        }
    }
    CFRelease(serverTrust);
    
    return finalTrustDecision;
}

@end

@interface SSPSPKIHashCache ()

// Dictionnary to cache SPKI hashes instead of having to compute them on every connection
@property (nonatomic) NSMutableDictionary<NSData *, NSData *> *spkiCache;
@property (nonatomic) dispatch_queue_t lockQueue;
@end

// These are the ASN1 headers for the Subject Public Key Info section of a certificate
// TODO(AD): Are they returned by the new iOS API https://developer.apple.com/documentation/security/2963103-seccertificatecopykey ?
static const unsigned char rsa2048Asn1Header[] =
{
    0x30, 0x82, 0x01, 0x22, 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86,
    0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x82, 0x01, 0x0f, 0x00
};

static const unsigned char rsa4096Asn1Header[] =
{
    0x30, 0x82, 0x02, 0x22, 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86,
    0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x82, 0x02, 0x0f, 0x00
};

static const unsigned char ecDsaSecp256r1Asn1Header[] =
{
    0x30, 0x59, 0x30, 0x13, 0x06, 0x07, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x02,
    0x01, 0x06, 0x08, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x03, 0x01, 0x07, 0x03,
    0x42, 0x00
};

static const unsigned char ecDsaSecp384r1Asn1Header[] =
{
    0x30, 0x76, 0x30, 0x10, 0x06, 0x07, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x02,
    0x01, 0x06, 0x05, 0x2b, 0x81, 0x04, 0x00, 0x22, 0x03, 0x62, 0x00
};



static char *getAsn1HeaderBytes(NSString *publicKeyType, NSNumber *publicKeySize)
{
    if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeRSA]) && ([publicKeySize integerValue] == 2048))
    {
        return (char *)rsa2048Asn1Header;
    }
    else if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeRSA]) && ([publicKeySize integerValue] == 4096))
    {
        return (char *)rsa4096Asn1Header;
    }
    else if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"10.0"))
    {
        if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeECSECPrimeRandom]) && ([publicKeySize integerValue] == 256))
        {
            return (char *)ecDsaSecp256r1Asn1Header;
        }
        else if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeECSECPrimeRandom]) && ([publicKeySize integerValue] == 384))
        {
            return (char *)ecDsaSecp384r1Asn1Header;
        }
    }
    
    @throw([NSException
            exceptionWithName:@"Unsupported public key algorithm"
            reason:[NSString stringWithFormat: @"Tried to generate the SPKI hash for an unsupported key algorithm %@ %@", publicKeyType, publicKeySize]
            userInfo:nil]);
}

static unsigned int getAsn1HeaderSize(NSString *publicKeyType, NSNumber *publicKeySize)
{
    if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeRSA]) && ([publicKeySize integerValue] == 2048))
    {
        return sizeof(rsa2048Asn1Header);
    }
    else if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeRSA]) && ([publicKeySize integerValue] == 4096))
    {
        return sizeof(rsa4096Asn1Header);
    }
    else if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"10.0"))
    {
        if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeECSECPrimeRandom]) && ([publicKeySize integerValue] == 256))
        {
            return sizeof(ecDsaSecp256r1Asn1Header);
        }
        else if (([publicKeyType isEqualToString:(NSString *)kSecAttrKeyTypeECSECPrimeRandom]) && ([publicKeySize integerValue] == 384))
        {
            return sizeof(ecDsaSecp384r1Asn1Header);
        }
    }
    
    @throw([NSException
            exceptionWithName:@"Unsupported public key algorithm"
            reason:[NSString stringWithFormat: @"Tried to generate the SPKI hash for an unsupported key algorithm %@ %@", publicKeyType, publicKeySize]
            userInfo:nil]);
}

namespace
{
    struct InternalKeyData
    {
        NSData* publicKeyData = nullptr;
        NSString *publicKeyType = nullptr;
        NSNumber *publicKeysize = nullptr;
    };
}

static InternalKeyData getInternalKeyDataCompat(SecKeyRef publicKey)
{
    InternalKeyData res;
    if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"10.0"))
    {
        NSData *publicKeyData = (__bridge_transfer NSData *)SecKeyCopyExternalRepresentation(publicKey, NULL);
        if (publicKeyData == nil)
        {
            NSLog(@"Error - could not extract the public key bytes");
            return res;
        }
        
        // Obtain the SPKI header based on the key's algorithm
        CFDictionaryRef publicKeyAttributes = SecKeyCopyAttributes(publicKey);
        NSString *publicKeyType = (__bridge NSString*)CFDictionaryGetValue(publicKeyAttributes, kSecAttrKeyType);
        NSNumber *publicKeysize = (__bridge NSNumber*)CFDictionaryGetValue(publicKeyAttributes, kSecAttrKeySizeInBits);
        CFRelease(publicKeyAttributes);
        
        res.publicKeyData = publicKeyData;
        res.publicKeyType = publicKeyType;
        res.publicKeysize = publicKeysize;
    }
    else
    {
        NSData* tmpTag = [[[NSUUID UUID] UUIDString] dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary* addParams = @{
            (id)kSecClass:(id)kSecClassKey,
            (id)kSecAttrApplicationTag: tmpTag,
            (id)kSecReturnData: (id)kCFBooleanTrue,
            (id)kSecReturnAttributes: (id)kCFBooleanTrue,
            (id)kSecValueRef: (__bridge id)publicKey
        };
        CFDictionaryRef queryData = nil;
        
        OSStatus addStatus = SecItemAdd((__bridge CFDictionaryRef)addParams, (CFTypeRef*)&queryData);
        if (addStatus != errSecSuccess || queryData == nil)
        {
            NSLog(@"Error - could not extract the public key bytes");
            return res;
        }
    
        NSData* publicKeyData = [(__bridge NSData *)CFDictionaryGetValue(queryData, kSecValueData) copy];
        NSString *publicKeyType = [NSString stringWithFormat:@"%@", (__bridge NSNumber*)CFDictionaryGetValue(queryData, kSecAttrKeyType)];
        NSNumber *publicKeysize = (__bridge NSNumber*)CFDictionaryGetValue(queryData, kSecAttrKeySizeInBits);
        CFRelease(queryData);
    
        NSDictionary* deleteParams = @{
            (id)kSecClass: (id)kSecClassKey,
            (id)kSecAttrApplicationTag: tmpTag
        };
        SecItemDelete((__bridge CFDictionaryRef)deleteParams);

        res.publicKeyData = publicKeyData;
        res.publicKeyType = publicKeyType;
        res.publicKeysize = publicKeysize;
    }
    return res;
}

@implementation SSPSPKIHashCache

- (instancetype)init
{
    self = [super init];
    if (self) {
        // Initialize our locks
        _lockQueue = dispatch_queue_create("TSKSPKIHashLock", DISPATCH_QUEUE_CONCURRENT);
        _spkiCache = [NSMutableDictionary new];
    }
    return self;
}

- (NSData *)hashSubjectPublicKeyInfoFromCertificate:(SecCertificateRef)certificate
{
    __block NSData *cachedSubjectPublicKeyInfo;
    
    // Have we seen this certificate before? Look for the SPKI in the cache
    NSData *certificateData = (__bridge_transfer NSData *)(SecCertificateCopyData(certificate));
    
    dispatch_sync(_lockQueue, ^{
        cachedSubjectPublicKeyInfo = self->_spkiCache[certificateData];
    });
    
    if (cachedSubjectPublicKeyInfo)
    {
        return cachedSubjectPublicKeyInfo;
    }
    
    // We didn't this certificate in the cache so we need to generate its SPKI hash
    
    // First extract the public key
    SecKeyRef publicKey = [self copyPublicKeyFromCertificate:certificate];
    
    // Obtain the public key bytes from the key reference
    /*NSData *publicKeyData = (__bridge_transfer NSData *)SecKeyCopyExternalRepresentation(publicKey, NULL);
    if (publicKeyData == nil)
    {
        NSLog(@"Error - could not extract the public key bytes");
        CFRelease(publicKey);
        return nil;
    }
    
    // Obtain the SPKI header based on the key's algorithm
    CFDictionaryRef publicKeyAttributes = SecKeyCopyAttributes(publicKey);
    NSString *publicKeyType = (__bridge NSString*)CFDictionaryGetValue(publicKeyAttributes, kSecAttrKeyType);
    NSNumber *publicKeysize = (__bridge NSNumber*)CFDictionaryGetValue(publicKeyAttributes, kSecAttrKeySizeInBits);
    CFRelease(publicKeyAttributes);*/
    
    InternalKeyData keyData = getInternalKeyDataCompat(publicKey);
    if (keyData.publicKeyData == nil || keyData.publicKeyType == nil || keyData.publicKeyType == nil)
    {
        NSLog(@"Error - could not extract the public key bytes");
        CFRelease(publicKey);
        return nil;
    }
    
    char *asn1HeaderBytes = getAsn1HeaderBytes(keyData.publicKeyType, keyData.publicKeysize);
    unsigned int asn1HeaderSize = getAsn1HeaderSize(keyData.publicKeyType, keyData.publicKeysize);
    
    CFRelease(publicKey);
    
    // Generate a hash of the subject public key info
    NSMutableData *subjectPublicKeyInfoHash = [NSMutableData dataWithLength:CC_SHA256_DIGEST_LENGTH];
    CC_SHA256_CTX shaCtx;
    CC_SHA256_Init(&shaCtx);
    
    // Add the missing ASN1 header for public keys to re-create the subject public key info
    CC_SHA256_Update(&shaCtx, asn1HeaderBytes, asn1HeaderSize);
    
    
    // Add the public key
    CC_SHA256_Update(&shaCtx, [keyData.publicKeyData bytes], (unsigned int)[keyData.publicKeyData length]);
    CC_SHA256_Final((unsigned char *)[subjectPublicKeyInfoHash bytes], &shaCtx);
    
    
    // Store the hash in our memory cache
    dispatch_barrier_sync(_lockQueue, ^{
        self->_spkiCache[certificateData] = subjectPublicKeyInfoHash;
    });
    
    return subjectPublicKeyInfoHash;
}

#pragma mark Public Key Converter - iOS 10.0+, macOS 10.12+, watchOS 3.0, tvOS 10.0

- (SecKeyRef)copyPublicKeyFromCertificate:(SecCertificateRef)certificate
{
    // Create an X509 trust using the using the certificate
    SecTrustRef trust;
    SecPolicyRef policy = SecPolicyCreateBasicX509();
    SecTrustCreateWithCertificates(certificate, policy, &trust);
    
    // Get a public key reference for the certificate from the trust
    SecTrustResultType result;
    SecTrustEvaluate(trust, &result);
    SecKeyRef publicKey = SecTrustCopyPublicKey(trust);
    CFRelease(policy);
    CFRelease(trust);
    return publicKey;
}

@end

NSDictionary* SaySslPinnerConfig(NSDictionary* domainPinningPolicy)
{
    // Convert settings supplied by the user to a configuration dictionary that can be used by SaySslPinner
    // This includes checking the sanity of the settings and converting public key hashes/pins from an
    // NSSArray of NSStrings (as provided by the user) to an NSSet of NSData
    NSMutableDictionary *domainFinalConfiguration = [[NSMutableDictionary alloc]init];
    
    
    // Always start with the optional excludeSubDomain setting; if it set, no other TSKDomainConfigurationKey can be set for this domain
    NSNumber *shouldExcludeSubdomain = domainPinningPolicy[kSSPExcludeSubdomainFromParentPolicy];
    if (shouldExcludeSubdomain != nil && [shouldExcludeSubdomain boolValue])
    {
        // Confirm that no other SSPDomainConfigurationKeys were set for this domain
        if ([[domainPinningPolicy allKeys] count] > 1)
        {
            [NSException raise:@"SaySslPinner configuration invalid"
                        format:@"SaySslPinner was initialized with SSPExcludeSubdomainFromParentPolicy but detected additional configuration keys"];
        }
        
        // Store the whole configuration and continue to the next domain entry
        domainFinalConfiguration[kSSPExcludeSubdomainFromParentPolicy] = @(YES);
        return [NSDictionary dictionaryWithDictionary:domainFinalConfiguration];
    }
    else
    {
        // Default setting is NO
        domainFinalConfiguration[kSSPExcludeSubdomainFromParentPolicy] = @(NO);
    }
    
    
    // Extract the optional includeSubdomains setting
    NSNumber *shouldIncludeSubdomains = domainPinningPolicy[kSSPIncludeSubdomains];
    if (shouldIncludeSubdomains == nil)
    {
        // Default setting is NO
        domainFinalConfiguration[kSSPIncludeSubdomains] = @(NO);
    }
    else
    {
        domainFinalConfiguration[kSSPIncludeSubdomains] = shouldIncludeSubdomains;
    }
    
    
    // Extract the optional expiration date setting
    NSString *expirationDateStr = domainPinningPolicy[kSSPExpirationDate];
    if (expirationDateStr != nil)
    {
        // Convert the string in the yyyy-MM-dd format into an actual date in UTC
        NSDateFormatter *dateFormat = [[NSDateFormatter alloc] init];
        dateFormat.dateFormat = @"yyyy-MM-dd";
        dateFormat.timeZone = [NSTimeZone timeZoneForSecondsFromGMT:0];
        NSDate *expirationDate = [dateFormat dateFromString:expirationDateStr];
        domainFinalConfiguration[kSSPExpirationDate] = expirationDate;
    }
    
    
    // Extract the optional enforcePinning setting
    NSNumber *shouldEnforcePinning = domainPinningPolicy[kSSPEnforcePinning];
    if (shouldEnforcePinning != nil)
    {
        domainFinalConfiguration[kSSPEnforcePinning] = shouldEnforcePinning;
    }
    else
    {
        // Default setting is YES
        domainFinalConfiguration[kSSPEnforcePinning] = @(YES);
    }
    
    // Extract and convert the subject public key info hashes
    NSArray<NSString *> *serverSslPinsBase64 = domainPinningPolicy[kSSPPublicKeyHashes];
    NSMutableSet<NSData *> *serverSslPinsSet = [NSMutableSet set];
    
    for (NSString *pinnedKeyHashBase64 in serverSslPinsBase64) {
        NSData *pinnedKeyHash = [[NSData alloc] initWithBase64EncodedString:pinnedKeyHashBase64 options:(NSDataBase64DecodingOptions)0];
        
        if ([pinnedKeyHash length] != CC_SHA256_DIGEST_LENGTH)
        {
            // The subject public key info hash doesn't have a valid size
            [NSException raise:@"SaySslPinner configuration invalid"
                        format:@"SaySslPinner was initialized with an invalid Pin %@", pinnedKeyHashBase64];
        }
        
        [serverSslPinsSet addObject:pinnedKeyHash];
    }
    
    
    /*NSUInteger requiredNumberOfPins = [domainFinalConfiguration[kSSPEnforcePinning] boolValue] ? 2 : 1;
    if([serverSslPinsSet count] < requiredNumberOfPins)
    {
        [NSException raise:@"SaySslPinner configuration invalid"
                    format:@"SaySslPinner was initialized with less than %lu pins (ie. no backup pins) for domain %@. This might brick your App; please review the Getting Started guide in ./docs/getting-started.md", (unsigned long)requiredNumberOfPins, domainName];
    }*/
    
    // Save the hashes for this server as an NSSet for quick lookup
    domainFinalConfiguration[kSSPPublicKeyHashes] = [NSSet setWithSet:serverSslPinsSet];
            
    return [NSDictionary dictionaryWithDictionary:domainFinalConfiguration];
}
