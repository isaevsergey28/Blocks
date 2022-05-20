//
//  SayUnityStateController.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 1.06.21.
//

#import <Foundation/Foundation.h>
#import "SayUnityStateController.h"


#ifdef __cplusplus
extern "C" {
#endif
    void UnityPause(int pause);
#ifdef __cplusplus
}
#endif


@implementation SayUnityStateController

static BOOL Paused = NO;

+ (void) CheckUnityPause:(BOOL) pause
{
    if(Paused != pause)
    {
        [SayUnityStateController UnityPause:pause];
        [[SayKitEvent sharedInstance] track:@"max_unity_state" param1:0 param2:5 extra:@"resumed" param3:0 param4:0 extra2:@"bug" tag:@""];
    }
}

+ (void) UnityPause:(BOOL) pause
{
    if(Paused == pause)
    {
        return;
    }
    
    Paused = pause;
    UnityPause(pause);
}

@end
