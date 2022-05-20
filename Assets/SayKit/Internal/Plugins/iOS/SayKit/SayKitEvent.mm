#import "SayKitEvent.h"
#import "SayKitLog.h"
#import "SayEndpoint.h"
#import "SaySslPinner.h"

#import <FirebaseCrashlytics/FirebaseCrashlytics.h>

NSString * const SAYKIT_EVENT_INTERSTITAL_IMP = @"interstitial_imp";
NSString * const SAYKIT_EVENT_REWARDED_IMP = @"rewarded_imp";

static const NSSet* const PRIORITY_EVENTS = [[NSSet alloc] initWithArray:@[
    @"app_start",
    @"unity_engine",
    @"crash_report",
    @"level_completed",
    @"level_failed",
    @"level_started",
    @"bonus_level_completed",
    @"bonus_level_started",
    @"bonus_level_failed",
    @"iap_android",
    @"interstitial_imp",
    @"rewarded_imp"
]];

static const NSArray* const PRIORITY_PREFIXES = @[
    @"imp_",
    @"ltv_"
];

static BOOL isPriorityEvent(NSString* event)
{
    if ([PRIORITY_EVENTS member:event])
    {
        return YES;
    }
    
    for (NSString* prefix in PRIORITY_PREFIXES)
    {
        if ([event hasPrefix:prefix])
        {
            return YES;
        }
    }
    
    return NO;
}

@implementation SayKitEvent

#pragma mark NSObject

// Manager to be used for methods that do not require a specific adunit to operate on.
+ (SayKitEvent*)sharedInstance
{
    static SayKitEvent* sharedSayKitEvent = [[SayKitEvent alloc] init];
    return sharedSayKitEvent;
}

- (SayKitEvent*)init
{
    if (self = [super init])
    {
        _sequence = 0;
        
        _endpoint = [[SayEndpoint alloc] initWithName:@"SayKitEvent"];
        _appKey = [[NSString alloc] init];
        _idfa = [[NSString alloc] init];
        _device_id = [[NSString alloc] init];
        _device_os = [[NSString alloc] init];
        _device_name = [[NSString alloc] init];
        _version = [[NSString alloc] init];
        
        _segment = 0;
        _level = 0;
        _scurrency = 0;
        _hcurrency = 0;
        
        _sessionUpdatedAt = 0;
        
        
        _bundle = [[NSBundle mainBundle] bundleIdentifier];
        _version = [NSString stringWithFormat:@"%@", [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"]];
        _idfv = [[[UIDevice currentDevice] identifierForVendor] UUIDString];
        
        [_endpoint addSslPin:@{
                     kSSPPublicKeyHashes: @[@"4a6cPehI7OG6cuDZka5NDZ7FR8a60d3auda+sKfg4Ng="],
                     kSSPEnforcePinning: @YES,
                     kSSPIncludeSubdomains: @YES
                   }
                   forDomain:@"saygames.io"];
        [_endpoint open];
        [_endpoint autoFlush:5 * 1000];

        
        [self setupScreenshotNotification];
        
    }
    
    return self;
}



- (NSString*)session
{
    NSTimeInterval now = [[NSDate date] timeIntervalSince1970];
    if (now - _sessionUpdatedAt > 120) {

        [[SayKitLog sharedInstance] Log:@"Starting new session"];
        
        NSString *letters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        int length = 16;
        
        NSMutableString* newSession = [NSMutableString stringWithCapacity: length];
        for (int i=0; i<length; i++) {
            [newSession appendFormat: @"%C", [letters characterAtIndex: arc4random_uniform([letters length])]];
        }
        _sessionId = newSession;
        
        [[FIRCrashlytics crashlytics] setCustomValue:[_sessionId copy] forKey:@"session" ];
        [[SayURLProtocolManager sharedInstance] updateSessionId:newSession];
    }
    _sessionUpdatedAt = now;
    return _sessionId;
}

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
        hcurrency:(int)hcurrency
{
    @synchronized (self) {
        _appKey = appKey;
        _idfa = idfa;
        _device_id = device_id;
        _device_os = device_os;
        _device_name = device_name;
        
        if (segment > 0) {
            _version = [NSString stringWithFormat:@"%@.%d", version, segment];
        } else {
            _version = version;
        }
        
        _segment = segment;
        _level = level;
        _scurrency = scurrency;
        _hcurrency = hcurrency;
    }
    
    [[FIRCrashlytics crashlytics] setUserID:device_id];
    
    NSString* url = [NSString stringWithFormat:@"https://track.saygames.io/events/%@", _appKey];
    [_endpoint setUrl:[NSURL URLWithString:url]];
    [self track:event param1:param1 param2:param2 extra:extra param3:param3 param4:param4 extra2:extra2 tag:tag];
    
    [[SayURLProtocolManager sharedInstance] updateEventParameters:appKey idfa:idfa device_os:device_os device_name:device_name];
}

- (void)trackImmediately:(NSString*)event
                  param1:(int)param1
                  param2:(int)param2
                   extra:(NSString*)extra
{
    [self track:event param1:param1 param2:param2 extra:extra param3:0 param4:0 extra2:@"" tag:@"" priority:YES];
    [_endpoint flushRequests];
}

- (void)track:(NSString*)event
       param1:(int)param1
       param2:(int)param2
        extra:(NSString*)extra
       param3:(int)param3
       param4:(int)param4
       extra2:(NSString*)extra2
          tag:(NSString*)tag
{
    [self track:event param1:param1 param2:param2 extra:extra param3:param3 param4:param4 extra2:extra2 tag:tag priority:NO];
}

- (void)track:(NSString*)event
       param1:(int)param1
       param2:(int)param2
        extra:(NSString*)extra
       param3:(int)param3
       param4:(int)param4
       extra2:(NSString*)extra2
          tag:(NSString*)tag
     priority:(BOOL)priority
{
    
    NSDateFormatter *dateFormatter=[[NSDateFormatter alloc] init];
    [dateFormatter setDateFormat:@"yyyy-MM-dd HH:mm:ss"];
    NSString* clientTime = [dateFormatter stringFromDate:[NSDate date]];

    @try {
        NSDictionary *eventData = NULL;
        @synchronized (self) {
             ++_sequence;
            eventData = @{
              @"time" : clientTime,
              @"sequence" : [NSNumber numberWithInt:_sequence],
              @"session" : [self session],
              @"idfa" : _idfa,
              @"device_id" : _device_id,
              @"device_os" : _device_os,
              @"device_name" : _device_name,
              @"version" : _version,
              @"event" : [NSString stringWithString:event],
              @"param1" : [NSNumber numberWithInt:param1],
              @"param2" : [NSNumber numberWithInt:param2],
              @"extra" : [NSString stringWithString:extra],
              @"param3" : [NSNumber numberWithInt:param3],
              @"param4" : [NSNumber numberWithInt:param4],
              @"extra2" : [NSString stringWithString:extra2],
              @"tag" : [NSString stringWithString:tag],
              @"level" : [NSNumber numberWithInt:_level],
              @"scurrency" : [NSNumber numberWithInt:_scurrency],
              @"hcurrency" : [NSNumber numberWithInt:_hcurrency]
            };
        }

        
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:eventData options:0 error:&error];
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"trackEvent %@\n", jsonString]];
    
        priority = priority || isPriorityEvent(event);
    
        [_endpoint addRequest:jsonString withBatching:SayEndpointBatch.appendWithNewLine withPriority:priority];
    
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"trackEvent error: %@\n", exception.reason]];
        NSError* error = [NSError errorWithDomain:@"SayKitEvent" code:1 userInfo:@{
            NSLocalizedFailureReasonErrorKey: exception.reason
        }];
        [[FIRCrashlytics crashlytics] recordError:error];
    }
}

- (void) setCrashlyticsParam:(NSString*)paramName paramValue:(NSString*)paramValue
{
    [[FIRCrashlytics crashlytics] setCustomValue:paramName forKey:paramValue];
}

- (void) trackFirebaseEvent:(NSString*)event extra:(NSString*)extra
{
    [FIRAnalytics logEventWithName:event
                        parameters:@{
                                     @"extra": extra
                                     }];
}

- (void) trackFirebaseEventWithValue:(NSString*)event value:(float) value addCurrency:(bool)addCurrency
{
    if (addCurrency)
    {
        [FIRAnalytics logEventWithName:event
        parameters:@{
            kFIRParameterValue: [NSNumber numberWithFloat:value],
            kFIRParameterCurrency: @"USD"
        }];
    }
    else
    {
        [FIRAnalytics logEventWithName:event
                            parameters:@{
                                kFIRParameterValue: [NSNumber numberWithFloat:value]
                            }];
    }
}

- (void) trackFullFirebaseEvent:(NSString*)logEvent valueToSum:(float)valueToSum dictJSPN:(NSString*)customJSPN
{
    NSMutableDictionary *data = [self convertCustomJSPNToDictionary:customJSPN];
    
    if(valueToSum != 0)
    {
        [data setObject:[NSNumber numberWithFloat:valueToSum] forKey:kFIRParameterValue];
    }

    [FIRAnalytics logEventWithName:logEvent
                           parameters:data];
}


- (void) trackFacebookEvent:(NSString*)event extra:(NSString*)extra
{
    NSDictionary *params = @{
        @"extra" : extra,
        @"version" : @"i2",
        @"advertiser_tracking_enabled" : @(advertiserTrackingEnabledFlag)
    };
    [FBSDKAppEvents logEvent:event parameters:params];
}

- (void) trackFacebookPurchaseEvent:(float)valueToSum currencyCode:(NSString*)currencyCode
{
    NSDictionary *params = @{
        @"fb_currency" : currencyCode,
        @"version" : @"i2",
        @"advertiser_tracking_enabled" : @(advertiserTrackingEnabledFlag)
    };
    
    [FBSDKAppEvents logEvent:@"fb_mobile_purchase" valueToSum:valueToSum parameters:params];
}

- (void) trackFullFacebookEvent:(NSString*)logEvent valueToSum:(float)valueToSum dictJSPN:(NSString*)customJSPN
{
    NSMutableDictionary *data = [self convertCustomJSPNToDictionary:customJSPN];
    
    [data setObject:@"i2" forKey:@"version"];
    [data setObject:@(advertiserTrackingEnabledFlag) forKey:@"advertiser_tracking_enabled"];
    
    if(valueToSum == 0)
    {
        [FBSDKAppEvents logEvent:logEvent parameters:data];
    }
    else {
        [FBSDKAppEvents logEvent:logEvent valueToSum:valueToSum parameters:data];
    }
}



- (NSMutableDictionary *) convertCustomJSPNToDictionary:(NSString*)customJSPN
{
    NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
    
    @try {
        if (customJSPN.length > 0) {
            while (customJSPN.length > 0) {
                
                NSRange part = [customJSPN rangeOfString:@"%|%"];
                
                NSString *key = [customJSPN substringToIndex:part.location];
                customJSPN = [customJSPN substringFromIndex:part.location + 3];
                
                part = [customJSPN rangeOfString:@"%|%"];
                NSString *value = [customJSPN substringToIndex:part.location];
                customJSPN = [customJSPN substringFromIndex:part.location + 3];
                
                part = [customJSPN rangeOfString:@"}&%&{"];
                NSString *typeValue = [customJSPN substringToIndex:part.location];
                customJSPN = [customJSPN substringFromIndex:part.location + 5];
                
                if ([typeValue isEqual:@"bool"]) {
                    [data setObject:[NSNumber numberWithBool:[value boolValue]] forKey:key];
                } else if ([typeValue isEqual:@"int"]) {
                    [data setObject:[NSNumber numberWithLong:[value longLongValue]] forKey:key];
                } else if ([typeValue isEqual:@"float"]) {
                    [data setObject:[NSNumber numberWithFloat:[value floatValue]] forKey:key];
                } else {
                    [data setObject:value forKey:key];
                }
                
            }
        }
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"trackFullFacebookEvent error: %@\n", exception.reason]];
        NSError* error = [NSError errorWithDomain:@"SayKitEvent" code:1 userInfo:@{
            NSLocalizedFailureReasonErrorKey: exception.reason
        }];
        
        [[FIRCrashlytics crashlytics] recordError:error];
    }
    
    return data;
}


- (void) trackDebugEvent:(NSString*)event adapter:(NSString*)adapter param_int1:(int)param_int1 param_str1:(NSString*)param_str1
{
    @try {
        
        int time = (int)[[NSDate date] timeIntervalSince1970];
        
        NSMutableDictionary *data = [[NSMutableDictionary alloc] init];
        
        [data setObject:_bundle forKey:@"bundle"];
        [data setObject:_version forKey:@"version"];
        [data setObject:_idfa forKey:@"idfa"];
        [data setObject:_idfv forKey:@"idfv"];
        
        [data setObject:@"ios" forKey:@"platform"];
        [data setObject:_device_os forKey:@"os"];
        [data setObject:_device_name forKey:@"device"];
        
        [data setObject:_appKey forKey:@"app_key"];
        [data setObject:_version forKey:@"app_version"];
        
        [data setObject:[NSNumber numberWithInt:time] forKey:@"client_ts"];
        
        
        [data setObject:event forKey:@"event"];
        [data setObject:adapter forKey:@"adapter"];
        [data setObject:[NSNumber numberWithInt:param_int1] forKey:@"param_int1"];
        [data setObject:param_str1 forKey:@"param_str1"];
        
        
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:&error];
        
        NSString *postLength = [NSString stringWithFormat:@"%lu", (unsigned long)[jsonData length]];
        
        NSMutableURLRequest *request = [[NSMutableURLRequest alloc] init];
        [request setURL:[NSURL URLWithString:@"https://track.saygames.io/mediation/waterfall-debug"]];
        [request setHTTPMethod:@"POST"];
        [request setValue:postLength forHTTPHeaderField:@"Content-Length"];
        [request setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
        [request setHTTPBody:jsonData];
        
        NSURLSession *session = [NSURLSession sharedSession];
        NSURLSessionDataTask *task = [session dataTaskWithRequest:request];
        [task resume];
        
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"trackDebugEvent: %@", exception.reason]];
    }
    
}



-(void) setupScreenshotNotification {

    NSOperationQueue *mainQueue = [NSOperationQueue mainQueue];
    [[NSNotificationCenter defaultCenter] addObserverForName:UIApplicationUserDidTakeScreenshotNotification
                                                      object:nil
                                                       queue:mainQueue
                                                  usingBlock:^(NSNotification *note) {
        
        [self track:@"screenshot" param1:0 param2:0 extra:@"" param3:0 param4:0 extra2:@"" tag:@"" priority:NO];
    }];
    
}

- (void) setFacebookAdvertiserTrackingEnabledFlag: (NSInteger) trackingEnabledFlag
{
    advertiserTrackingEnabledFlag = trackingEnabledFlag;
}

@end
