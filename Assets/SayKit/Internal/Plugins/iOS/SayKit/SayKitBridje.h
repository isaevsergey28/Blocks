//
//  SayKitBridje.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 8/31/20.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface SayKitBridje : NSObject

+ (SayKitBridje*)sharedManager;
    
@property int ApplicationStartTimestamp;


+ (void)sendUnityIDFAPopupEvent:(NSArray*)args;
+ (void)sendUnityIDFARedirectToSettingsEvent;

@end

NS_ASSUME_NONNULL_END
