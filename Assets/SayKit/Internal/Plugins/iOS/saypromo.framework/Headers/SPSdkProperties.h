//#import "UnityServices.h"
#import "SPDeviceLog.h"
@import Foundation;

@interface SPSdkProperties : NSObject

FOUNDATION_EXPORT NSString * const kSayPromoServicesCacheDirName;
FOUNDATION_EXPORT NSString * const kSayPromoServicesLocalCacheFilePrefix;
FOUNDATION_EXPORT NSString * const kSayPromoServicesLocalStorageFilePrefix;
FOUNDATION_EXPORT NSString * const kSayPromoServicesWebviewBranchInfoDictionaryKey;
FOUNDATION_EXPORT NSString * const kSayPromoServicesVersionName;
FOUNDATION_EXPORT int const kSayPromoServicesVersionCode;
FOUNDATION_EXPORT NSString * const kSayPromoServicesFlavorDebug;
FOUNDATION_EXPORT NSString * const kSayPromoServicesFlavorRelease;

+ (BOOL)isInitialized;
+ (void)setInitialized:(BOOL)initialized;
+ (BOOL)isTestMode;
+ (void)setTestMode:(BOOL)testmode;
+ (int)getVersionCode;
+ (NSString *)getVersionName;
+ (NSString *)getCacheDirectoryName;
+ (NSString *)getCacheFilePrefix;
+ (NSString *)getLocalStorageFilePrefix;
+ (void)setConfigUrl:(NSString *)configUrl;
+ (NSString *)getConfigUrl;
+ (NSString *)getDefaultConfigUrl:(NSString *)flavor;
+ (NSString *)getLocalInterstitialWebViewFile;
+ (NSString *)getLocalInterstitialVideoFile;
+ (NSString *)getLocalRewardedWebViewFile;
+ (NSString *)getLocalRewardedVideoFile;
+ (NSString *)getCacheDirectory;
+ (NSString *)mraidJavascript;
+ (NSString *)getImagePath:(NSString *) name;
+ (void)setInitializationTime:(long long)milliseconds;
+ (long long)getInitializationTime;
+ (void)setReinitialized:(BOOL)status;
+ (BOOL)isReinitialized;
+ (void)setDebugMode:(BOOL)enableDebugMode;
+ (BOOL)getDebugMode;
//+ (id<UnityServicesDelegate>)getDelegate;
//+ (void)setDelegate:(id<UnityServicesDelegate>)delegate;

@end
