@import Foundation;
@import SystemConfiguration;
@import CoreFoundation;

#import <netinet/in.h>
#import "SPConnectivityDelegate.h"

@interface SPConnectivityMonitor : NSObject

+ (void)setConnectionMonitoring:(BOOL)status;

+ (void)startListening:(id<SPConnectivityDelegate>)connectivityDelegate;

+ (void)stopListening:(id<SPConnectivityDelegate>)connectivitydelegate;

+ (void)stopAll;

@end


