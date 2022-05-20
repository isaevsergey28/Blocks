//
//  SayAppDelegate.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 9/25/20.
//

#import "UnityAppController.h"
#import "SayURLProtocol.h"
#import "SayURLProtocolExtension.h"
#import "SayKitBridje.h"

#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import "SayUnityStateController.h"


@interface SayAppDelegate : UnityAppController
-(void) onScheduledTimer:(NSTimer *)timer;
-(void) checkControllerName:(NSString *) controllerName;
@end

IMPL_APP_CONTROLLER_SUBCLASS(SayAppDelegate)


@implementation SayAppDelegate

-(BOOL)application:(UIApplication*) application didFinishLaunchingWithOptions:(NSDictionary*) options
{
    int timestamp = round([[NSDate date] timeIntervalSince1970]);
    [SayKitBridje sharedManager].ApplicationStartTimestamp = timestamp;
    
    
    [NSURLProtocol wk_registerScheme:@"http"];
    [NSURLProtocol wk_registerScheme:@"https"];
    
    [NSURLProtocol registerClass:[SayURLProtocol class]];
    
    [NSTimer scheduledTimerWithTimeInterval:1 target:self selector:@selector(onScheduledTimer:) userInfo:nil repeats:YES];
    
    // From 9.0.0, developers are required to initialize the SDK explicitly with the initializeSDK method or implicitly by calling it in applicationDidFinishLaunching.
    [[FBSDKApplicationDelegate sharedInstance] application:application
                               didFinishLaunchingWithOptions:options];
    
    return [super application:application didFinishLaunchingWithOptions:options];
}

-(void) onScheduledTimer:(NSTimer *)timer {
    
    UIViewController *controller = [[[UIApplication sharedApplication] delegate] window].rootViewController.presentedViewController;
    
    if(controller){
        NSString* controllerName = NSStringFromClass([controller class]);
        [self checkControllerName:controllerName];
    }
    else
    {
        [SayUnityStateController CheckUnityPause:NO];
        [self checkControllerName:@"UnityAppController"];
    }
}

NSString* _presentedControllerName;
-(void) checkControllerName:(NSString *) controllerName
{
    if(![_presentedControllerName isEqualToString:controllerName])
    {
        _presentedControllerName = controllerName;
        
        [[SayKitEvent sharedInstance] track:@"controller" param1:0 param2:0 extra:controllerName param3:0 param4:0 extra2:@"" tag:@""];
    }
}


@end
