#import <UIKit/UIKit.h>
#import <AVFoundation/AVFoundation.h>

@protocol SPVideoViewDelegate <NSObject>

- (void)onPlayerTouched;

@end

@interface SPVideoView : UIView

@property (nonatomic) AVPlayer *player;

@property (nonatomic, weak) id<SPVideoViewDelegate> delegate;

+ (Class)layerClass;
- (void)setPlayer:(AVPlayer *)player;
- (void)setVideoFillMode:(NSString *)fillMode;
- (AVPlayer*)player;

- (void)initTapGesture:(id<SPVideoViewDelegate>)delegate;

@end
