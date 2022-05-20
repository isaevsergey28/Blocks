//
//  SayKitVersionManager.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 9/4/19.
//

#import "SayKitVersionManager.h"

@implementation SayKitVersionManager


// Manager to be used for methods that do not require a specific adunit to operate on.
+ (SayKitVersionManager*)sharedInstance
{
    static SayKitVersionManager* sharedSayVersionManager = nil;
    
    if (!sharedSayVersionManager)
    {
        sharedSayVersionManager = [[SayKitVersionManager alloc] init];
    }
    
    return sharedSayVersionManager;
}

static NSMutableDictionary * sdkVersionDictionary;


- (void) trackAdNetworkVersions {
    
    @try {
        
        NSString* sayPromoVersion =  [[SPAdManager sharedInstance] getVersion];
        [self trackSDKVersion:@"sdk_saypromo" version:sayPromoVersion];
        
        NSString* adColonyVersion = [AdColony getSDKVersion];
        [self trackSDKVersion:@"sdk_adColony" version:adColonyVersion];
        
        NSString* appLovinVersion = ALSdk.version;
        [self trackSDKVersion:@"sdk_applovin" version:appLovinVersion];
        
        NSString* facebookVersion = FB_AD_SDK_VERSION;
        [self trackSDKVersion:@"sdk_facebook" version:facebookVersion];
        
        NSString* ironSourceVersion = [IronSource sdkVersion];
        [self trackSDKVersion:@"sdk_ironsource" version:ironSourceVersion];
        
        NSString* byteDanceVersion = [BUAdSDKManager SDKVersion];
        [self trackSDKVersion:@"sdk_bytedance" version:byteDanceVersion];
        
        NSString* unityAdsVersion = [UnityAds getVersion];
        [self trackSDKVersion:@"sdk_unity" version:unityAdsVersion];
        
        NSString* vungleVersion =VungleSDKVersion;
        [self trackSDKVersion:@"sdk_vungle" version:vungleVersion];
        
        NSString* mintegralVersion = MTGSDK.sdkVersion;
        [self trackSDKVersion:@"sdk_mintegral" version:mintegralVersion];
        
        NSString* fyberVersion = IASDKCore.sharedInstance.version;
        [self trackSDKVersion:@"sdk_fyber" version:fyberVersion];
        
        NSString* inMobiVersion = [IMSdk getVersion];
        [self trackSDKVersion:@"sdk_inmobi" version:inMobiVersion];
        
        
        [self trackSDKVersion:@"sdk_yandex" version:@"4.3.0"];
        
        NSString* mytargerVersion = [MTRGVersion currentVersion];
        [self trackSDKVersion:@"sdk_mytarget" version:fyberVersion];
        
        
//        NSString* apsVersion = DTBAds.version;
//        NSArray * apsVersionArray = [[NSArray alloc] init];
//        apsVersionArray = [apsVersion componentsSeparatedByString:@"-"];
//        apsVersion = apsVersionArray[apsVersionArray.count - 1];
        
        [self trackSDKVersion:@"sdk_aps" version:@"3.4.4"];//apsVersion
        
        NSString* admobVersion =  [NSString stringWithUTF8String:(char*)GoogleMobileAdsVersionString];
        NSArray * admobVersionArray = [[NSArray alloc] init];
        admobVersionArray = [admobVersion componentsSeparatedByString:@"-v"];
        admobVersion = admobVersionArray[admobVersionArray.count - 1];
        
        [self trackSDKVersion:@"sdk_admob" version:admobVersion];
        
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"checkAdNetworkVersions error: %@\n", exception.reason]];
    }
    
}

- (void) trackSDKVersion:(NSString*)sdkName
                 version:(NSString*)version
{
    [SayKitEvent.sharedInstance track:sdkName
                               param1:0
                               param2:0
                                extra:version
                               param3:0
                               param4:0
                               extra2:@""
                                  tag:@""];
}


@end
