//
//  SayKitIDFAManager.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 8/28/20.
//

#import "SayKitIDFAManager.h"

@implementation SayKitIDFAManager

NSString* const sayKitIDFAManagerAuthorized = @"Authorized";
NSString* const sayKitIDFAManagerDenied = @"Denied";


-(void) showNativeIDFAPopup:(NSString*)title
                description:(NSString*)description
                okBtnString:(NSString*)okBtnString
            cancelBtnString:(NSString*)cancelBtnString {
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:[NSString stringWithString:title]
                                                                             message:[NSString stringWithString:description]
                                                                      preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction *actionOk = [UIAlertAction actionWithTitle:[NSString stringWithString:okBtnString]
                                                       style:UIAlertActionStyleDefault
                                                     handler:^(UIAlertAction * action) {
        [self showSystemIDFAPopup];
    }];
    
    UIAlertAction *actionCancel = [UIAlertAction actionWithTitle:[NSString stringWithString:cancelBtnString]
                                                           style:UIAlertActionStyleDefault
                                                         handler:^(UIAlertAction * action) {
        [self idfaPopupResponse:sayKitIDFAManagerDenied];
    }];
    
    [alertController addAction:actionCancel];
    [alertController addAction:actionOk];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}


- (NSString*) getTrackingAuthorizationStatus {
    if (@available(iOS 14.5, *)) {

        if ([ATTrackingManager trackingAuthorizationStatus] == ATTrackingManagerAuthorizationStatusRestricted)
        {
            [self updateFBAudienceAdvertiserTrackingFlag:NO];
            return @"ATTrackingManagerAuthorizationStatusRestricted";
        }
        else if ([ATTrackingManager trackingAuthorizationStatus] == ATTrackingManagerAuthorizationStatusAuthorized)
        {
            [self updateFBAudienceAdvertiserTrackingFlag:YES];
            return @"ATTrackingManagerAuthorizationStatusAuthorized";
        }
        else if ([ATTrackingManager trackingAuthorizationStatus] == ATTrackingManagerAuthorizationStatusDenied)
        {
            [self updateFBAudienceAdvertiserTrackingFlag:NO];
            return @"ATTrackingManagerAuthorizationStatusDenied";
        }
        else{
            [self updateFBAudienceAdvertiserTrackingFlag:NO];
            return @"ATTrackingManagerAuthorizationStatusNotDetermined";
        }
    }
    else
    {
        return @"ATTrackingManagerAuthorizationStatusAuthorized";
    }
}

-(void) showSystemIDFAPopup {
    
    if (@available(iOS 14, *)) {

        if ([ATTrackingManager trackingAuthorizationStatus] != ATTrackingManagerAuthorizationStatusNotDetermined)
        {
            [self redirectToSettings];
        }
        else
        {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {

                NSLog(@"ATTrackingManager requested");


                if( status == ATTrackingManagerAuthorizationStatusAuthorized)
                {
                    [self idfaPopupResponse:sayKitIDFAManagerAuthorized];
                }
                else
                {
                    [self idfaPopupResponse:sayKitIDFAManagerDenied];
                }
            }];
        }
    }
}


-(void) redirectToSettings
{
    [SayKitBridje sendUnityIDFARedirectToSettingsEvent];
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
}

-(void) idfaPopupResponse:(NSString*)message
{
    if([message isEqualToString: sayKitIDFAManagerAuthorized])
    {
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        [defaults setObject:@"ATTrackingManagerAuthorizationStatusAuthorized" forKey:@"ATTrackingManagerAuthorizationStatus"];
    }
    else
    {
        NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
        [defaults setObject:@"ATTrackingManagerAuthorizationStatusDenied" forKey:@"ATTrackingManagerAuthorizationStatus"];
    }
    
    [SayKitBridje sendUnityIDFAPopupEvent:@[message]];
}

-(void) updateFBAudienceAdvertiserTrackingFlag:(BOOL)advertiserTrackingEnabled;
{
    //If you are using mediation, then you need to implement the setAdvertiserTrackingEnabled flag before initializing the mediation SDK in order for us to receive it in the bidding request.
    [FBAdSettings setAdvertiserTrackingEnabled:advertiserTrackingEnabled];
    
    if(advertiserTrackingEnabled){
        [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:1];
    }
    else{
        [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:0];
    }
}

- (void) updateConversionValue:(NSInteger) value
{
    if (@available(iOS 14.0, *)) {
        [SKAdNetwork updateConversionValue:value];
    }
}

@end
