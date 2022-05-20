#import "SPDeviceLog.h"
#import <AVFoundation/AVFoundation.h>
#import "SPBaseManager.h"

@interface SPAVPlayer : AVPlayer

@property (nonatomic, strong) NSString *url;
@property (nonatomic, assign) int progressInterval;
@property (nonatomic, assign) BOOL isPlaying;
@property (nonatomic, assign) SPBaseManager *spBaseManager;

@property id timeObserverToken;

- (void)setProgressEventInterval:(int)progressEventInterval;
- (void)prepare:(NSString *)url initialVolume:(float)volume timeout:(NSInteger)timeout baseManager:(SPBaseManager *) baseManager;
- (void)stop;
- (void)stopObserving;
- (void)seekTo:(long)msec;

- (long) getCurrentPosition;
- (long) getDuration;

- (void) trackDebugEvent:(NSString *)event;

- (void)addPeriodicTimeObserver:(long)videoPlayerPosition;

@end
