//
//  SayKitBridje.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 8/31/20.
//

#import "SayKitBridje.h"

@implementation SayKitBridje

+ (SayKitBridje*)sharedManager
{
    static SayKitBridje* sharedManager = nil;

    if (!sharedManager)
        sharedManager = [[SayKitBridje alloc] init];

    return sharedManager;
}

+ (void)sendUnityIDFAPopupEvent:(NSArray*)args
{
    NSData* data = [NSJSONSerialization dataWithJSONObject:args options:0 error:nil];
    UnitySendMessage("SayKitBridje", (@"IDFAPopupShowedEvent").UTF8String, [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding].UTF8String);
}

+ (void)sendUnityIDFARedirectToSettingsEvent
{
    UnitySendMessage("SayKitBridje", (@"IDFARedirectToSettings").UTF8String, @"".UTF8String);
}

@end
