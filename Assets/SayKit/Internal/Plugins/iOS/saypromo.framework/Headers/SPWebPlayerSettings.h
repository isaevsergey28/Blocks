#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, SayPromoWebPlayerWebSettings) {
    kSayPromoWebPlayerWebSettingsAllowsInlineMediaPlayback,
    kSayPromoWebPlayerWebSettingsMediaPlaybackRequiresUserAction,
    kSayPromoWebPlayerWebSettingsTypesRequiringAction,
    kSayPromoWebPlayerWebSettingsScalesPagesToFit,
    kSayPromoWebPlayerWebSettingsJavaScriptEnabled,
    kSayPromoWebPlayerWebSettingsJavaScriptCanOpenWindowsAutomatically,
    kSayPromoWebPlayerWebSettingsMediaPlaybackAllowsAirPlay,
    kSayPromoWebPlayerWebSettingsSuppressesIncrementalRendering,
    kSayPromoWebPlayerWebSettingsKeyboardDisplayRequiresUserAction,
    kSayPromoWebPlayerWebSettingsIgnoresViewportScaleLimits,
    kSayPromoWebPlayerWebSettingsDataDetectorTypes,
    kSayPromoWebPlayerWebSettingsScrollEnabled
};

NSString *SPNSStringFromWebPlayerWebSetting(SayPromoWebPlayerWebSettings);
