#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, SayPromoWebPlayerEvent) {
    kSayPromoWebPlayerPageStarted,
    kSayPromoWebPlayerPageFinished,
    kSayPromoWebPlayerError,
    kSayPromoWebPlayerEvent,
    kSayPromoWebPlayerShouldOverrideURLLoading,
    kSayPromoWebPlayerCreateWebView
};

NSString *SPNSStringFromWebPlayerEvent(SayPromoWebPlayerEvent);
