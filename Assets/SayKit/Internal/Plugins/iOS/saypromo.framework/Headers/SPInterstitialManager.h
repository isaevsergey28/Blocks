//
//  SPInterstitialManager.h
//  saypromo
//
//  Created by Timur Dularidze on 3/28/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPBaseManager.h"
#import "SPAVPlayer.h"

NS_ASSUME_NONNULL_BEGIN

@interface SPInterstitialManager : SPBaseManager

@property (nonatomic, strong) SPAVPlayer *videoPlayer;

- (void)requestInterstitial : (NSDictionary *)info delegate:(id<SayPromoAdsDelegate>)delegate;
- (void)loadInterstitial: (NSDictionary *)info jsonData:(NSString *)jsonData delegate:(id<SayPromoAdsDelegate>)delegate;

- (void)readJsonDataAndInit: (NSString *)jsonData;

- (void)showInterstitial;



- (BOOL) isAdsReady;

- (void) DownloadStart:(NSString *)adType;
- (void) DownloadEnd:(NSString *)adType length:(long)length;
- (void) DownloadError:(NSString *)adType error:(NSString *)error;

@end

NS_ASSUME_NONNULL_END
