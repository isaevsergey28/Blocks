//
//  SPRewardedManager.h
//  saypromo
//
//  Created by Timur Dularidze on 4/4/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPBaseManager.h"
#import "SPAVPlayer.h"

NS_ASSUME_NONNULL_BEGIN

@interface SPRewardedManager : SPBaseManager

@property (nonatomic, strong) SPAVPlayer *videoPlayer;

- (void)requestRewardedVideo : (NSDictionary *)info delegate:(id<SayPromoAdsDelegate>)delegate;
- (void)loadRewardedVideo : (NSDictionary *)info jsonData:(NSString *)jsonData delegate:(id<SayPromoAdsDelegate>)delegate;

- (void)readJsonDataAndInit: (NSString *)jsonData;

- (void)showRewardedVideo;



- (BOOL) isAdsReady;

- (void) DownloadStart:(NSString *)adType;
- (void) DownloadEnd:(NSString *)adType length:(long)length;
- (void) DownloadError:(NSString *)adType error:(NSString *)error;
@end

NS_ASSUME_NONNULL_END
