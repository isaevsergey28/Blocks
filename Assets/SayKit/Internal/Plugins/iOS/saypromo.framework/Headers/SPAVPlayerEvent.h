@import Foundation;

typedef NS_ENUM(NSInteger, SayPromoAdsAVPlayerEvent) {
    kSayPromoAdsAVPlayerEventPrepared,
    kSayPromoAdsAVPlayerEventProgress,
    kSayPromoAdsAVPlayerEventCompleted,
    kSayPromoAdsAVPlayerEventSeekTo,
    kSayPromoAdsAVPlayerEventLikelyToKeepUp,
    kSayPromoAdsAVPlayerEventBufferEmpty,
    kSayPromoAdsAVPlayerEventBufferFull,
    kSayPromoAdsAVPlayerEventPlay,
    kSayPromoAdsAVPlayerEventPause,
    kSayPromoAdsAVPlayerEventStop
};

NSString *SPNSStringFromAVPlayerEvent(SayPromoAdsAVPlayerEvent);

typedef NS_ENUM(NSInteger, SayPromoAdsAVPlayerError) {
    kSayPromoAdsAVPlayerPrepareError,
    kSayPromoAdsAVPlayerPrepareTimeout,
    kSayPromoAdsAVPlayerGenericError
};

NSString *SPNSStringFromAVPlayerError(SayPromoAdsAVPlayerError);
