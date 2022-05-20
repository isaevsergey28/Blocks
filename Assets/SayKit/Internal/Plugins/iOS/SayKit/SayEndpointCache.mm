#import "SayEndpointCache.h"
#import "SayEndpoint.h"
#import "SayKitEvent.h"

#import <sqlite3.h>
#import <CommonCrypto/CommonCryptor.h>
#import <CommonCrypto/CommonDigest.h>

#include <string>

static constexpr int VERSION = 0;

static void crashlyticsLog(NSString* msg, int code)
{
    NSError* error = [NSError errorWithDomain:@"SayEndpointCache" code:code userInfo:@{
        NSLocalizedDescriptionKey: msg
    }];
    [[FIRCrashlytics crashlytics] recordError:error];
}

static void crashlyticsLog(NSString* msg, NSException* e, int code)
{
    NSError* error = [NSError errorWithDomain:@"SayEndpointCache" code:code userInfo:@{
        NSLocalizedDescriptionKey: msg,
        NSLocalizedFailureReasonErrorKey: e.reason
    }];
    [[FIRCrashlytics crashlytics] recordError:error];
}

static std::string obfuscate(const char* name)
{
    NSString* string = [NSString stringWithUTF8String:name];
    NSData* data = [string dataUsingEncoding:NSUTF8StringEncoding];
    NSString* base64 = [data base64EncodedStringWithOptions:0];
    std::string result([base64 UTF8String]);
    while (result.back() == '=') {
        result.pop_back();
    }
    std::replace(result.begin(), result.end(), '+', '-');
    std::replace(result.begin(), result.end(), '/', '_');
    return result;
}

static std::string dbFolder()
{
    //[NSBundle mainBundle] pa
    NSArray* paths = NSSearchPathForDirectoriesInDomains(NSLibraryDirectory, NSUserDomainMask, YES);
    NSString* basePath = paths[0];
    return std::string([basePath UTF8String]) + "/" + obfuscate("endpoints");
}

static std::string dbPath(NSString* name)
{
    return dbFolder() + "/" + obfuscate([name UTF8String]) + ".db";
}

static void finalize(sqlite3_stmt* stm)
{
    if (stm != NULL)
    {
        sqlite3_finalize(stm);
    }
}

static sqlite3_stmt* prepare(sqlite3* db, const char* statement)
{
    sqlite3_stmt* compiled = NULL;
    BOOL res = sqlite3_prepare_v2(db, statement, -1 , &compiled, NULL);
    if (res != SQLITE_OK || compiled == NULL)
    {
        int extendedError = sqlite3_extended_errcode(db);
        finalize(compiled);
        crashlyticsLog([NSString stringWithFormat:@"Can't compile query %s\n%s", statement, sqlite3_errmsg(db)], -extendedError);
        return NULL;
    }
    return compiled;
}

static bool execute(sqlite3* db, sqlite3_stmt* stm)
{
    if (stm != NULL)
    {
        int res = sqlite3_step(stm);
        if (res != SQLITE_DONE)
        {
            int extendedError = sqlite3_extended_errcode(db);
            crashlyticsLog([NSString stringWithFormat:@"Can't execute query\n%s", sqlite3_errmsg(db)], -extendedError);
            return false;
        }
    }
    return true;
}

static void executeRaw(sqlite3* db, const char* statement)
{
    sqlite3_stmt* compiled = prepare(db, statement);
    execute(db, compiled);
    finalize(compiled);
}

template<typename Callback>
static void selectRaw(sqlite3* db, const char* statement, Callback&& callback)
{
    sqlite3_stmt* compiled = prepare(db, statement);

    if (compiled == NULL)
    {
        return;
    }
    
    while(sqlite3_step(compiled) == SQLITE_ROW)
    {
        callback(compiled);
    }
    finalize(compiled);
}

static std::string key(const char* left, const char* right);

static void doCleanup(std::string path);

@implementation SayEndpointCache
{
    NSString* _name;
    sqlite3* _db;
    NSMutableDictionary* _batchingMap;
}

-(instancetype)initWithName:(NSString*) name
{
    if (self = [self init])
    {
        _name = [name copy];
        _db = NULL;
        _batchingMap = [NSMutableDictionary new];
    }
    
    return self;
}
-(NSArray*)open
{
    [self prepareDb];
    
    NSMutableArray* result = [NSMutableArray new];
    
    if (_db == NULL)
    {
        return  result;
    }
    
    selectRaw(_db, "SELECT ordering, body, batching, priority FROM requests ORDER BY ordering",
    [=](sqlite3_stmt* stm)
    {
        @try
        {
            int ordering = sqlite3_column_int(stm, 0);
            const void* cipheredBody = sqlite3_column_blob(stm, 1);
            int cipheredBodySize = sqlite3_column_bytes(stm, 1);
            NSString* body = [self decipher:cipheredBody size:cipheredBodySize];
            if (body == nil)
            {
                crashlyticsLog(@"Can't decipher request body", 3);
                return;
            }
            
            auto batchingClass = sqlite3_column_text(stm, 2);
            BOOL priority = sqlite3_column_int(stm, 3);
            NSObject<ISayEndpointBatching>* batching = [self getBatching: batchingClass];
            
            SayEndpointStringRequest* request = [[SayEndpointStringRequest alloc]
                                                 initWithBody:body
                                                 order:ordering
                                                 batching:batching
                                                 priority:priority];
            [result addObject:request];
        }
        @catch(NSException* e)
        {
            crashlyticsLog(@"Failed to read request", e, 15);
        }
    });
    
    return result;
}

-(void)dealloc
{
    if (_db != NULL)
    {
        sqlite3_close(_db);
    }
#if !__has_feature(objc_arc)
    [super dealloc];
#endif
}

-(void)cacheReques:(SayEndpointStringRequest*) data
{
    if (_db == NULL)
    {
        crashlyticsLog(@"SayEndpointCache is not opened", 4);
        return;
    }
    
    auto stm = prepare(_db, "INSERT INTO requests(ordering, body, batching, priority) VALUES(?, ?, ?, ?)");
    if (stm == NULL)
    {
        return;
    }
    
    std::pair<std::unique_ptr<char[]>, int> ciphered = [self cipher:[data getBody]];
    
    if (!ciphered.first || ciphered.second == 0)
    {
        crashlyticsLog(@"Failed to cipher request body", 5);
        return;
    }
    
    sqlite3_bind_int(stm, 1, [data getOrder]);
    sqlite3_bind_blob(stm, 2, ciphered.first.get(), ciphered.second, SQLITE_TRANSIENT);
    sqlite3_bind_text(stm, 3, [NSStringFromClass([[data getBatching] class]) UTF8String], -1, SQLITE_TRANSIENT);
    sqlite3_bind_int(stm, 4, [data isPriority]);
    
    execute(_db, stm);
    finalize(stm);
}

-(void)removeRequestsLessOrEqual:(int) order
{
    if (_db == NULL)
    {
        crashlyticsLog(@"SayEndpointCache is not opened", 6);
        return;
    }
    
    auto stm = prepare(_db, "DELETE FROM requests WHERE ordering <= ?");
    if (stm == NULL)
    {
        [self purgeDb];
        return;
    }
    
    sqlite3_bind_int(stm, 1, order);
    
    bool executed = execute(_db, stm);
    finalize(stm);
    
    if (!executed)
    {
        [self purgeDb];
    }
}

-(void)removeRequest:(SayEndpointStringRequest*) data
{
    if (_db == NULL)
    {
        crashlyticsLog(@"SayEndpointCache is not opened", 7);
        return;
    }
    
    auto stm = prepare(_db, "DELETE FROM requests WHERE ordering = ?");
    if (stm == NULL)
    {
        [self purgeDb];
        return;
    }
    
    sqlite3_bind_int(stm, 1, [data getOrder]);
    
    bool executed = execute(_db, stm);
    finalize(stm);
    
    if (!executed)
    {
        [self purgeDb];
    }
}

-(void)prepareDb
{
    NSError* error = NULL;
    [[NSFileManager defaultManager]
     createDirectoryAtPath:[NSString stringWithUTF8String:dbFolder().c_str()]
     withIntermediateDirectories: NO
     attributes:@{}
     error:&error];
    if (error != NULL && error.code != 516) {
        [[FIRCrashlytics crashlytics] recordError:error];
    }
    
    std::string path = dbPath(_name);
    
    doCleanup(path);
    
    int openResult = sqlite3_open(path.c_str(), &_db);
    if (openResult != SQLITE_OK) {
        sqlite3_close(_db);
        _db = NULL;
        return;
    }
    
    executeRaw(_db, "CREATE TABLE IF NOT EXISTS __info (k TEXT PRIMARY KEY, v TEXT)");
    int storedVersion = VERSION;
    
    selectRaw(_db, "SELECT v FROM __info WHERE k = \"version\"",
        [&storedVersion](sqlite3_stmt* stm)
        {
            const char* versionStr = (const char*)sqlite3_column_text(stm, 0);
            if (versionStr != NULL)
            {
                storedVersion = atoi(versionStr);
            }
        });
    
    if (storedVersion == VERSION)
    {
        [self onDbOpened];
    }
    else
    {
        [self onDbUpgradeFrom: storedVersion to: VERSION];
    }
}

-(void)onDbOpened
{
    executeRaw(_db, "CREATE TABLE IF NOT EXISTS requests (ordering INTEGER PRIMARY KEY, body BLOB, batching TEXT, priority INTEGER)");
    [self saveCurrentVersion];
}

-(void)purgeDb
{
    //remove db in the case of inconsistent state
    if (_db == NULL) return;
    
    sqlite3_close(_db);
    _db = NULL;
    
    NSFileManager *fileManager = [NSFileManager defaultManager];
    NSError* removeError = nil;
    NSString* path = [NSString stringWithUTF8String:dbPath(_name).c_str()];
    BOOL removed = [fileManager removeItemAtPath:path error:&removeError];
    if (!removed && removeError != nil)
    {
        [[FIRCrashlytics crashlytics] recordError:removeError];
    }
    
    [self prepareDb];
}

-(void)onDbUpgradeFrom:(int)versionFrom to:(int)versionTo
{
    //TODO upgrade
    [self saveCurrentVersion];
}

-(void)saveCurrentVersion
{
    auto stm = prepare(_db, "INSERT OR REPLACE INTO __info(k, v) VALUES(\"version\", ?)");
    if (stm == NULL)
    {
        return;
    }
    sqlite3_bind_text(stm, 1, std::to_string(VERSION).c_str(), -1, SQLITE_TRANSIENT);
    execute(_db, stm);
    finalize(stm);
}

-(NSObject<ISayEndpointBatching>*)getBatching:(const unsigned char*)name
{
    if (name == NULL || *name == 0)
    {
        return NULL;
    }
    
    NSString* key = [NSString stringWithUTF8String:(const char *)name];
    NSObject<ISayEndpointBatching>* batching = [_batchingMap objectForKey:key];
    if (batching != NULL)
    {
        return batching;
    }
    
    @try {
        batching = [[NSClassFromString(key) alloc] init];
        _batchingMap[key] = batching;
        return batching;
    } @catch (NSException *exception) {
        crashlyticsLog([NSString stringWithFormat:@"Can't create instance of %@", key], 8);
    }
    
    return NULL;
}

-(NSString*)decipher:(const void*)rawData size:(int)size
{
    using byte = char;
    const byte* data = (const byte*)rawData;
    const byte* md5 = data;
    int32_t ivSize = kCCBlockSizeAES128;
    const byte* iv = data + CC_MD5_DIGEST_LENGTH;
    const byte* bodyIn = iv + ivSize;
    int bodySize = size - CC_MD5_DIGEST_LENGTH - kCCBlockSizeAES128;
    std::string key = ::key("3*>g3n\"&(Q<xBFbTE7]~:!>RXtj?^?6Q", "CF-P:sUj4czhZq/A{%e{\"pa[u?*~\"2)\"");
    
    if (bodySize <= 0) {
        crashlyticsLog(@"Invalid body size", 12);
        return nil;
    }
    
    byte* buf = new byte[bodySize];
    size_t cryptBytes = 0;
    
    CCCryptorStatus status = CCCrypt(kCCDecrypt,
                                     kCCAlgorithmAES,
                                     kCCOptionPKCS7Padding,
                                     key.data(), key.size(),
                                     iv, bodyIn, size_t(bodySize),
                                     buf, size_t(bodySize), &cryptBytes);
    
    if (status != kCCSuccess)
    {
        delete[] buf;
        crashlyticsLog([NSString stringWithFormat:@"Can't decipher request body! Status: %d", status], 9);
        return nil;
    }
    byte digest[CC_MD5_DIGEST_LENGTH];
    CC_MD5(buf, (unsigned int)cryptBytes, (unsigned char*)digest);
    if (memcmp(md5, digest, CC_MD5_DIGEST_LENGTH) != 0)
    {
        delete[] buf;
        crashlyticsLog(@"Digest don't match", 13);
        return nil;
    }
    
    NSString* res = [[NSString alloc] initWithBytes:buf length:cryptBytes encoding:NSUTF8StringEncoding];
    delete [] buf;
    return res;
}

-(std::pair<std::unique_ptr<char[]>, int>)cipher:(NSString*)body
{
    int bodySize = int([body lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
    const char* bodyIn = [body UTF8String];
    int size = CC_MD5_DIGEST_LENGTH + kCCBlockSizeAES128 * 2 + bodySize;
    using byte = char;
    byte* data = new byte[size];
    byte* md5 = data;
    int32_t ivSize = kCCBlockSizeAES128;
    byte* iv = data + CC_MD5_DIGEST_LENGTH;
    byte* bodyOut = iv + ivSize;
    std::string key = ::key("3*>g3n\"&(Q<xBFbTE7]~:!>RXtj?^?6Q", "CF-P:sUj4czhZq/A{%e{\"pa[u?*~\"2)\"");
    
    int ivStatus = SecRandomCopyBytes(kSecRandomDefault, ivSize, iv);
    if (ivStatus != 0) {
        delete[] data;
        crashlyticsLog(@"Can't generate iv", 14);
        return {};
    }
    CC_MD5(bodyIn, bodySize, (unsigned char*)md5);
    
    size_t cryptBytes = 0;
    CCCryptorStatus status = CCCrypt(kCCEncrypt,
                                     kCCAlgorithmAES,
                                     kCCOptionPKCS7Padding,
                                     key.data(), key.size(),
                                     iv, bodyIn, size_t(bodySize),
                                     bodyOut, size_t(bodySize) + kCCBlockSizeAES128, &cryptBytes);
    
    if (status != kCCSuccess)
    {
        delete[] data;
        crashlyticsLog(@"Can't cipher body", 10);
        return {};
    }
    
    std::unique_ptr<char[]> ptr((char*)data);
    return std::make_pair(std::move(ptr), CC_MD5_DIGEST_LENGTH + kCCBlockSizeAES128 + int(cryptBytes));
}

@end

static std::string key(const char* left, const char* right)
{
    std::string res = left;
    for (size_t i = 0; i < res.size(); ++i)
    {
        res[i] = res[i] ^ right[i];
    }
    return res;
}

#define CLEANUP_TAG @"1"
static void doCleanup(std::string path)
{
    NSFileManager *fileManager = [NSFileManager defaultManager];
    NSString* pathToFile = [NSString stringWithUTF8String:path.c_str()];
    NSString* cleanupTag = [NSString stringWithFormat:@"%@_v%@", pathToFile, CLEANUP_TAG];
    
    if (![fileManager fileExistsAtPath:cleanupTag])
    {
        BOOL created = [fileManager createFileAtPath:cleanupTag contents:nil attributes:nil];
        if (!created)
        {
            crashlyticsLog([NSString stringWithFormat:@"Can't create file %@", cleanupTag], 11);
            return;
        }
        NSError* removeError = nil;
        BOOL removed = [fileManager removeItemAtPath:pathToFile error:&removeError];
        if (!removed && removeError != nil && !([removeError.domain isEqualToString:NSCocoaErrorDomain] && removeError.code == 4))
        {
            [[FIRCrashlytics crashlytics] recordError:removeError];
        }
    }
}
