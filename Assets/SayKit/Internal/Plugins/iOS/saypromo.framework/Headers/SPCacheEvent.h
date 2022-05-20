@import Foundation;

typedef NS_ENUM(NSInteger, SayPromoServicesCacheEvent) {
    kSayPromoServicesDownloadStarted,
    kSayPromoServicesDownloadStopped,
    kSayPromoServicesDownloadEnd,
    kSayPromoServicesDownloadProgress,
    kSayPromoServicesDownloadError
};

NSString *SPNSStringFromCacheEvent(SayPromoServicesCacheEvent);
