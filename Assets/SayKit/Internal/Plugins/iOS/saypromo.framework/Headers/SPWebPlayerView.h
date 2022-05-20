#import "SPDeviceLog.h"
#import <WebKit/WebKit.h>
#import <UIKit/UIKit.h>
#import "SPSdkProperties.h"
#import "SPDebugLog.h"

@protocol SPWebPlayerDelegate <NSObject>

- (void)sayPromoWebPlayerOpenUrl:(NSString *)url;

@end

@interface SPWebPlayerView : UIView

@property (nonatomic, weak) id<SPWebPlayerDelegate> delegate;

- (instancetype)initWithFrame:(CGRect)frame
                       viewId:(NSString*)viewId
            webPlayerSettings:(NSDictionary*)webPlayerSettings
                       debugId:(NSString*)debugId
                       appId:(NSString *)appId
                        place:(NSString *)place
                     delegate:(id<SPWebPlayerDelegate>)delegate;

@property (nonatomic, strong) NSString *appId;
@property (nonatomic, strong) NSString *placeId;
@property (nonatomic, strong) NSString *debugId;
@property long htmlShowTimestamp;


- (void)loadFromLocalFile:(NSString*)fileUrl;
- (void)loadMRAID:(CGSize)size;

- (void)setWebPlayerSettings:(NSDictionary*)webPlayerSettings;
- (void)setEventSettings:(NSDictionary*)eventSettings;

- (void)destroy;
- (void) clearWebView;

- (void)fireReadyEvent;

- (void) trackDebugEvent:(NSString *)event
                    str1:(NSString *)str1
                    int1:(long)int1
                    int2:(long)int2
                    int3:(long)int3;
@end
