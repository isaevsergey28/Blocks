//
//  SPAdHandler.h
//  saypromo
//
//  Created by Timur Dularidze on 4/8/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPDeviceLog.h"
#import "SPGlobalData.h"
#import "SPAdManager.h"
//#import "SPInterstitialManager.h"
//#import "SPRewardedManager.h"

NS_ASSUME_NONNULL_BEGIN

@interface SPAdHandler : NSObject

+ (SPAdHandler *)sharedInstance;

- (void)trackEvent:(NSString *)eventId adType:(NSString *)adType length:(long)length error:(NSString *)error;
- (void)trackEvent:(NSString *)eventId category:(NSString *)category param1:(id)param1, ... NS_REQUIRES_NIL_TERMINATION;
- (void)trackEvent:(NSString *)eventId category:(NSString *)category params:(NSArray *)params;

@end

NS_ASSUME_NONNULL_END
