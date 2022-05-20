#import "SPDeviceLog.h"
#import <sys/sysctl.h>

@interface SPDevice : NSObject
+ (void) initCarrierUpdates;

+ (NSString *)getAppVersion;
    
+ (NSString *)getOsVersion;

+ (NSString *)getOsBuild;

+ (NSString *)getModel;

+ (NSString *)getDeviceName;

+ (NSString *)getBundleIdentifier;

+ (NSString *)getCurrentDate;

+ (BOOL) isDeviceX;

+ (BOOL)isSimulator;

+ (NSInteger)getScreenLayout;

+ (NSString *)getAdvertisingTrackingId;

+ (NSString *)getVendorTrackingId;

+ (BOOL)isLimitTrackingEnabled;

+ (BOOL)isUsingWifi;

+ (NSInteger)getNetworkType;

+ (NSString *)getNetworkOperator;

+ (NSString *)getNetworkOperatorName;

+ (float)getScreenScale;

+ (NSNumber *)getScreenWidth;

+ (NSNumber *)getScreenHeight;

+ (BOOL)isActiveNetworkConnected;

+ (NSString *)getUniqueEventId;

+ (BOOL)isWiredHeadsetOn;

+ (NSString *)getTimeZone:(BOOL) daylightSavingTime;

+ (NSInteger)getTimeZoneOffset;

+ (NSString *)getPreferredLocalization;

+ (float)getOutputVolume;

+ (float)getScreenBrightness;

+ (NSNumber *)getFreeSpaceInKilobytes;

+ (NSNumber *)getTotalSpaceInKilobytes;

+ (float)getBatteryLevel;

+ (NSInteger)getBatteryStatus;

+ (NSNumber *)getTotalMemoryInKilobytes;

+ (NSNumber *)getFreeMemoryInKilobytes;

+ (NSDictionary *)getProcessInfo;

+ (BOOL)isRooted;

+ (NSInteger)getUserInterfaceIdiom;

+ (NSArray<NSString *>*)getSensorList;

+ (NSString *)getGLVersion;

+ (float)getDeviceMaxVolume;

+ (NSUInteger)getCPUCount;

@end
