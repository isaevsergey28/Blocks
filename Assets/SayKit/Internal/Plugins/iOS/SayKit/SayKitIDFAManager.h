//
//  SayKitIDFAManager.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 8/28/20.
//

#import <Foundation/Foundation.h>
#import <StoreKit/SKAdNetwork.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>

#import "SayKitBridje.h"
#import "SayKitEvent.h"
#import <FBAudienceNetwork/FBAdSettings.h>


NS_ASSUME_NONNULL_BEGIN

@interface SayKitIDFAManager : NSObject


-(void) showNativeIDFAPopup:(NSString*)title
                description:(NSString*)description
                okBtnString:(NSString*)okBtnString
            cancelBtnString:(NSString*)cancelBtnString;

-(void) redirectToSettings;
-(void) showSystemIDFAPopup;

-(NSString*) getTrackingAuthorizationStatus;

-(void) idfaPopupResponse:(NSString*)message;

-(void) updateFBAudienceAdvertiserTrackingFlag:(BOOL)advertiserTrackingEnabled;

- (void) updateConversionValue:(NSInteger) value;

@end

NS_ASSUME_NONNULL_END
