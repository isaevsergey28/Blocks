//
//  SPAdManager.h
//  saypromo
//
//  Created by Timur Dularidze on 4/3/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPDeviceLog.h"
#import "SPInterstitialManager.h"
#import "SPRewardedManager.h"
#import "SPDebugLog.h"
#import "AdToken.h"

NS_ASSUME_NONNULL_BEGIN



@interface SPAdManager : NSObject


+ (SPAdManager *)sharedInstance;

@property (nonatomic, strong) SPInterstitialManager *interstitialManager;
@property (nonatomic, strong) SPRewardedManager *rewardedManager;


- (NSString *)getVersion;

- (NSString *)getMaxToken:(NSDictionary *)info;
- (void) loadMaxInterstitial:(NSDictionary *)info adMarkup:(NSString *)adMarkup delegate:(id<SayPromoAdsDelegate>)delegate;
- (void) loadMaxRewarded:(NSDictionary *)info adMarkup:(NSString *)adMarkup delegate:(id<SayPromoAdsDelegate>)delegate;


- (void) requestInterstitialWithCustomEventInfo:(NSDictionary *)info delegate:(id<SayPromoAdsDelegate>)delegate;
- (void) loadInterstitialWithCustomEventInfo:(NSDictionary *)info jsonData:(NSString *)jsonData delegate:(id<SayPromoAdsDelegate>)delegate;
- (void) showInterstitialWithCustomEventInfo:(NSString *)placementId delegate:(id<SayPromoAdsDelegate>)delegate;

- (void) requestRewardedVideoWithCustomEventInfo:(NSDictionary *)info delegate:(id<SayPromoAdsDelegate>)delegate;
- (void) loadRewardedWithCustomEventInfo:(NSDictionary *)info jsonData:(NSString *)jsonData delegate:(id<SayPromoAdsDelegate>)delegate;
- (void) presentRewardedVideoFromViewController:(NSString *)placementId delegate:(id<SayPromoAdsDelegate>)delegate;


- (void)setLogLevel:(SayPromoServicesLogLevel) logLevel;

- (BOOL) isInterstitialReady:(NSString *)placementId;
- (BOOL) isRewardedReady:(NSString *)placementId;

- (void) trackEvent:(NSString *)eventId adType:(NSString *)adType length:(long)length error:(NSString *)error;

@end

NS_ASSUME_NONNULL_END
