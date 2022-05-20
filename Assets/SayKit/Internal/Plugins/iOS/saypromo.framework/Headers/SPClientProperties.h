#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import "SKAdData.h"

@interface SPClientProperties : NSObject

+ (void)setGameId:(NSString *)gid;
+ (NSString *)getGameId;

+ (NSString *)getSaySKAdNetworkId;
+ (NSString *)getSaySignature:(NSData *) signature;

+ (NSDictionary<NSString *, id> *) getSKStoreProductParameters:(NSString *)skadJSON
                                          iTunesItemIdentifier:(NSInteger)iTunesItemIdentifier;


+ (NSArray<NSString*>*)getSupportedOrientationsPlist;
+ (int)getSupportedOrientations;

+ (BOOL) sayNetworkIdInPlist;
+ (BOOL) networkIdInPlist:(NSString *)skadNetworkId;

+ (NSString *)getAppName;
+ (NSString *)getAppVersion;

+ (BOOL)isAppDebuggable;

+ (void)setCurrentViewController:(UIViewController *)viewController;

+ (UIViewController *)getCurrentViewController;

@end
