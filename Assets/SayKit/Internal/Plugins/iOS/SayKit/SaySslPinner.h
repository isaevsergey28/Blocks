//
//  SaySslPinner.h
//  Unity-iPhone
//
//  Created by Nick Satchok on 11/22/19.
//  Base on opensource github project https://github.com/datatheorem/TrustKit
//

#ifndef SaySslPinner_h
#define SaySslPinner_h

extern NSString* _Nonnull const kSSPPublicKeyHashes;
extern NSString* _Nonnull const kSSPEnforcePinning;
extern NSString* _Nonnull const kSSPExcludeSubdomainFromParentPolicy;
extern NSString* _Nonnull const kSSPIncludeSubdomains;
extern NSString* _Nonnull const kSSPPublicKeyAlgorithms;
extern NSString* _Nonnull const kSSPExpirationDate;

@interface SaySslPinner : NSObject
-(instancetype)initWithConfig:(NSDictionary*_Nonnull)config;
- (BOOL)handleChallenge:(NSURLAuthenticationChallenge * _Nonnull)challenge
      completionHandler:
        (void (^ _Nonnull)(NSURLSessionAuthChallengeDisposition disposition, NSURLCredential * _Nullable credential))completionHandler;
@end

NSDictionary* _Nonnull SaySslPinnerConfig(NSDictionary* dic);

#endif /* SaySslPinner_h */
