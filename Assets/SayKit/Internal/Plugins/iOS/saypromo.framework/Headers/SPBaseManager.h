//
//  SPBaseManager.h
//  saypromo
//
//  Created by Timur Dularidze on 4/4/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPVideoView.h"
#import "SPWebPlayerView.h"
#import "SPWebRequest.h"
#import "AdData.h"
#import "CreativeData.h"
#import "SPCacheQueue.h"
#import "SPSdkProperties.h"
#import "SPGlobalData.h"
#import "SPDevice.h"
#import "SPUIViewController.h"
#import "SPDebugLog.h"
#import <StoreKit/StoreKit.h>

#import "SPClientProperties.h"

typedef enum : NSInteger {
    AdNoneType = 0,
    AdVideoType = 1,
    AdHtmlType = 2,
    AdVideoAndHtmlType = 3
} AdType;

typedef enum : NSInteger {
    AdNoneStateType = 0,
    AdLoadingStateType,
    AdLoadedStateType,
    AdNotFoundStateType,
    AdErrorStateType,
    AdPlayingStateType
} AdStateType;


typedef enum : NSInteger {
    Interstitial = 0,
    Reward = 1,
} AdNameType;

typedef NS_ENUM(NSInteger, SayPromoAdErrorCode)
{
    SayPromoAdErrorCodeInitializeFailed,
    SayPromoAdErrorCodeInternalError,
    SayPromoAdErrorCodeFileIOError,
    SayPromoAdErrorCodeNoFill
};

NS_ASSUME_NONNULL_BEGIN


@protocol SayPromoAdsDelegate <NSObject>

- (void)sayPromoAdsReady:(NSString *)placeId;

- (void)sayPromoAdsDidError:(NSInteger) sayPromoAdsError;

- (void)sayPromoAdsDidStart:(NSString *)placeId;

- (void)sayPromoAdsDidFinish:(NSString *)placeId;

- (void)sayPromoAdsDidClick:(NSString *)placementId;

@end

static NSString *const kSayPromoApplicationIdKey = @"appId";
static NSString *const kSayPromoPlacementIdKey = @"placeId";
static NSString *const SayPromoBaseUrl = @"https://api.saypromo.net/ad/request";

@interface SPBaseManager : NSObject

@property (nonatomic, weak) id<SayPromoAdsDelegate> delegate;


@property (nonatomic, strong) NSString *appId;
@property (nonatomic, strong) NSString *placeId;
@property (nonatomic, strong) NSString *videoUrl;
@property (nonatomic, strong) NSString *htmlUrl;

@property long htmlLoadTimestamp;
@property long videoLoadTimestamp;
@property (nonatomic, strong) NSString *debugId;

@property NSLock *_lock;

@property BOOL isForegroundState;
@property BOOL isVideoDownloaded;
@property BOOL isHtmlDownloaded;
@property NSInteger adType;
@property int adStateType;
@property long countdownCount;
@property long countdownState;
@property float closeBtnTimerCount;

@property (nonatomic, strong) AdData *adData;
@property (nonatomic, strong) SPVideoView *videoView;
@property (nonatomic, strong) SPWebPlayerView *webPlayerView;
@property (nonatomic, strong) SPUIViewController *controller;
@property (nonatomic, strong) UIButton *closeBtn;
@property (nonatomic, strong) UIView *btnBackView;
@property (nonatomic, strong) UIImageView *logoImage;
@property (nonatomic, strong) UILabel *countdown;
@property (nonatomic, strong) NSTimer *animationTimer;

@property long videoPlayerPosition;


@property (nonatomic, strong) UIView *progressBackView;
@property (nonatomic, strong) UIView *progressView;

@property CGRect def_mainFrame;
@property CGRect mainFrame;

@property NSInteger webSkipAfter;
@property NSInteger videoSkipAfter;
@property NSInteger videoClickableAfter;

@property BOOL isCloseBtnEnabled;
@property BOOL isProgressAnimationStarted;
@property BOOL isVideoShowed;

@property BOOL videoShowTimer;
@property BOOL webShowTimer;

@property BOOL isIPad;

@property BOOL videoShowProgressBar;
@property BOOL IsNeedToShowTimer;

@property NSString *videoBtnPosition;
@property NSString *webBtnPosition;
@property NSString *currentBtnPosition;

@property NSString *videoSayPosition;
@property NSString *webSayPosition;

@property NSString *skadNetworkJson;

@property float videoHeight;
@property float videoWidth;

@property float heightTopOffset;
@property float heightBottomOffset;
@property float contentHeightOffset;
@property float contentWidthOffset;


- (void) updateAdStateType:(NSInteger) newState;
- (NSString *) getStringAdStateType;

- (long) getCurrentVideoPosition;
- (long) getVideoDuration;

- (void) initProgressView;

- (void)videoIsCompleted;
- (NSString *) getURL:(NSInteger*)type;
- (void) sendEvent:(NSString *) url;
- (void) openURL;

-(void) startTimerForCloseBtn:(long)delay showTimer:(bool)showTimer;

-(void) updateBtnPosition:(NSString *)position;
-(void) updateLogoPosition:(NSString *)position;

-(void) updateCloseBtnState:(bool)isEnabled;
-(void) updateCountdownFrames;

-(void) cleanProgressAnimation;

- (void) cleanTimer;
- (void) cleanVideoPlayerData;

- (void) finishAdView;


- (NSData *) makeApiRequest:(NSString *) url;

- (void) trackImpression;

- (void) trackDebugEvent:(NSString *)event
                    str1:(NSString *)str1
                    int1:(long)int1
                    int2:(long)int2
                    int3:(long)int3;

- (CGFloat) pixelToPoints:(CGFloat)px;
- (CGFloat) pointsToPixels:(CGFloat)points;

@end

NS_ASSUME_NONNULL_END
