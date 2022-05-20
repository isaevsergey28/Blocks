#import "SPWebRequest.h"
#import "SPDeviceLog.h"
#import "SPAdHandler.h"

@interface SPCacheOperation : NSOperation

@property (nonatomic, strong) NSString *source;
@property (nonatomic, strong) NSString *target;
@property (nonatomic, assign) int connectTimeout;
@property (nonatomic, assign) int progressEventInterval;
@property (nonatomic, assign) id progressTimer;
@property (nonatomic, assign) long long lastProgressEvent;
@property (nonatomic, assign) long long expectedContentSize;
@property (nonatomic, strong) SPWebRequest *request;
@property (nonatomic, strong) NSDictionary<NSString*, NSArray*> *headers;
@property (nonatomic, assign) BOOL append;
@property (nonatomic, strong) NSString *adType;

- (instancetype)initWithSource:(NSString *)source target:(NSString *)target connectTimeout:(int)connectTimeout headers:(NSDictionary<NSString*, NSArray*> *)headers append:(BOOL)append adType:(NSString *)adType;
@end
