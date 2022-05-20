//
//  SayKitVersionManager.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 9/4/19.
//

#import <Foundation/Foundation.h>

#import <saypromo/SPAdManager.h>
#import <AdColony/AdColony.h>
#import <AppLovinSDK/AppLovinSDK.h>
#import <FBAudienceNetwork/FBAudienceNetwork.h>
#import <IronSource/IronSource.h>
#import <BUAdSDK/BUAdSDKManager.h>
#import <UnityAds/UnityAds.h>
#import <VungleSDK/VungleSDK.h>
#import <DTBiOSSDK/DTBiOSSDK.h>
#import <MTGSDK/MTGSDK.h>
#import <IASDKCore/IASDKCore.h>
#import <InMobiSDK/IMSdk.h>

#import <GoogleMobileAds/GoogleMobileAds.h>

#import <MyTargetSDK/MyTargetSDK.h>

#import "SayKitEvent.h"
#import "SayKitLog.h"

NS_ASSUME_NONNULL_BEGIN

@interface SayKitVersionManager : NSObject

+ (SayKitVersionManager*)sharedInstance;
- (void) trackAdNetworkVersions;
- (void) trackSDKVersion:(NSString*)sdkName
                 version:(NSString*)version;

@end

NS_ASSUME_NONNULL_END
