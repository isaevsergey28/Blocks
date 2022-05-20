
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "Firebase.h"
#import <FirebaseAnalytics/FirebaseAnalytics.h>
#import <FBSDKCoreKit/FBSDKCoreKit.h>

#import "SayURLProtocol.h"

@class SayEndpoint;
extern NSString * const SAYKIT_EVENT_INTERSTITAL_IMP;
extern NSString * const SAYKIT_EVENT_REWARDED_IMP;

@interface SayKitEvent : NSObject
{
@private
    SayEndpoint* _endpoint;

    int _sequence;
    
    NSString* _appKey;
    NSString* _idfa;
    NSString* _device_id;
    NSString* _device_os;
    NSString* _device_name;
    NSString* _version;
    int _segment;
    int _level;
    int _scurrency;
    int _hcurrency;
    
    NSTimeInterval _sessionUpdatedAt;
    NSString* _sessionId;    
    
    NSString* _bundle;
    NSString* _idfv;
    
    NSInteger advertiserTrackingEnabledFlag;
}

+ (SayKitEvent*)sharedInstance;

- (SayKitEvent*)init;
- (void)trackFull:(NSString*)appKey
             idfa:(NSString*)idfa
        device_id:(NSString*)device_id
        device_os:(NSString*)device_os
      device_name:(NSString*)device_name
          version:(NSString*)version
          segment:(int)segment
            event:(NSString*)event
           param1:(int)param1
           param2:(int)param2
            extra:(NSString*)extra
           param3:(int)param3
           param4:(int)param4
           extra2:(NSString*)extra2
              tag:(NSString*)tag
            level:(int)level
        scurrency:(int)scurrency
        hcurrency:(int)hcurrency;

- (void)track:(NSString*)event
       param1:(int)param1
       param2:(int)param2
        extra:(NSString*)extra
       param3:(int)param3
       param4:(int)param4
       extra2:(NSString*)extra2
          tag:(NSString*)tag;

- (void)trackImmediately:(NSString*)event
                  param1:(int)param1
                  param2:(int)param2
                   extra:(NSString*)extra;

- (NSString*)session;

- (NSMutableDictionary *) convertCustomJSPNToDictionary:(NSString*)customJSPN;

- (void) trackFirebaseEvent:(NSString*)event extra:(NSString*)extra;
- (void) trackFirebaseEventWithValue:(NSString*)event value:(float) value addCurrency:(bool)addCurrency;
- (void) trackFullFirebaseEvent:(NSString*)logEvent valueToSum:(float)valueToSum dictJSPN:(NSString*)customJSPN;

- (void) setCrashlyticsParam:(NSString*)paramName paramValue:(NSString*)paramValue;


- (void) trackFacebookEvent:(NSString*)event extra:(NSString*)extra;
- (void) trackFacebookPurchaseEvent:(float)valueToSum currencyCode:(NSString*)currencyCode;
- (void) trackFullFacebookEvent:(NSString*)logEvent valueToSum:(float)valueToSum dictJSPN:(NSString*)customJSPN;

- (void) trackDebugEvent:(NSString*)event adapter:(NSString*)adapter param_int1:(int)param_int1 param_str1:(NSString*)param_str1;

-(void) setupScreenshotNotification;

- (void) setFacebookAdvertiserTrackingEnabledFlag: (NSInteger) trackingEnabledFlag;

@end
