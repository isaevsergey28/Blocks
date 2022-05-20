#import <Foundation/Foundation.h>

typedef NS_ENUM(int, SayPromoServicesLogLevel) {
    kSayPromoServicesLogLevelError = 1,
    kSayPromoServicesLogLevelWarning = 2,
    kSayPromoServicesLogLevelInfo = 3,
    kSayPromoServicesLogLevelDebug = 4
};

#define SPLogCore(logLevel, logLevelStr, format, function, lineNumber, ...)\
    if (logLevel <= [SPDeviceLog getLogLevel]) NSLog((@"%@/SayAds: %s (line:%d) :: " format), logLevelStr, function, lineNumber, ##__VA_ARGS__);

#define SPLogError(fmt, ...) SPLogCore(kSayPromoServicesLogLevelError, @"E", fmt, __PRETTY_FUNCTION__, __LINE__, ##__VA_ARGS__);
#define SPLogWarning(fmt, ...) SPLogCore(kSayPromoServicesLogLevelWarning, @"W", fmt, __PRETTY_FUNCTION__, __LINE__, ##__VA_ARGS__);
#define SPLogInfo(fmt, ...) SPLogCore(kSayPromoServicesLogLevelInfo, @"I", fmt, __PRETTY_FUNCTION__, __LINE__, ##__VA_ARGS__);
#define SPLogDebug(fmt, ...) SPLogCore(kSayPromoServicesLogLevelDebug, @"D", fmt, __PRETTY_FUNCTION__, __LINE__, ##__VA_ARGS__);

@interface SPDeviceLog : NSObject

+ (void)setLogLevel:(SayPromoServicesLogLevel)logLevel;
+ (SayPromoServicesLogLevel)getLogLevel;

@end


