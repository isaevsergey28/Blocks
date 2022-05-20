
#import "SayKitEvent.h"
#import "SayKitLog.h"

#import <UIKit/UIKit.h>

#import "TenjinSDK.h"
#import <FBSDKCoreKit/FBSDKCoreKit.h>

#import <FirebaseCrashlytics/FirebaseCrashlytics.h>
#import <FirebaseInstallations/FIRInstallations.h>

#import <sys/utsname.h>
#import <AdSupport/ASIdentifierManager.h>

#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import "SayKitVersionManager.h"
#import <FBAudienceNetwork/FBAdSettings.h>

#import <mach/mach.h>
#import <mach/mach_host.h>

#import "SayKitIDFAManager.h"
#import "SayKitBridje.h"

#import <saypromo/SPClientProperties.h>


void sayKitCrashlyticsInit()
{
    [FIRApp configure];
    
    @try {
        NSDateFormatter *dateFormatter=[[NSDateFormatter alloc] init];
        [dateFormatter setDateFormat:@"yyyy-MM-dd HH:mm:ss"];
        NSString* clientTime = [dateFormatter stringFromDate:[NSDate date]];
        
        struct utsname systemInfo;
        uname(&systemInfo);
        
        NSString* device_name = [NSString stringWithCString:systemInfo.machine
                           encoding:NSUTF8StringEncoding];
        
        [[FIRCrashlytics crashlytics] setCustomValue:device_name forKey:@"device" ];
        [[FIRCrashlytics crashlytics] setCustomValue:clientTime forKey:@"start_time" ];
        
    } @catch (NSException *exception) {
       NSLog(@"sayKitCrashlyticsInit error: %@\n", exception.reason);
    }
}


void sayKitEventTrackFull(
        const char* appKey,
        const char* idfa,
        const char* device_id,
        const char* device_os,
        const char* device_name,
        const char* version,
        const int segment,
        const char* event,
        const int param1,
        const int param2,
        const char* extra,
        const int param3,
        const int param4,
        const char* extra2,
        const char* tag,
        const int level,
        const int scurrency,
        const int hcurrency)
{
    if (!extra){
        extra = "";
    }

    [[SayKitEvent sharedInstance] trackFull:[NSString stringWithUTF8String:appKey]
                                       idfa:[NSString stringWithUTF8String:idfa]
                                  device_id:[NSString stringWithUTF8String:device_id]
                                  device_os:[NSString stringWithUTF8String:device_os]
                                device_name:[NSString stringWithUTF8String:device_name]
                                    version:[NSString stringWithUTF8String:version]
                                    segment:segment
                                      event:[NSString stringWithUTF8String:event]
                                     param1:param1
                                     param2:param2
                                      extra:[NSString stringWithUTF8String:extra]
                                     param3:param3
                                     param4:param4
                                     extra2:[NSString stringWithUTF8String:extra2]
                                        tag:[NSString stringWithUTF8String:tag]
                                      level:level
                                  scurrency:scurrency
                                  hcurrency:hcurrency];
}

void sayKitEventTrackFirebase(
                              const char* event,
                              const char* extra)
{
    if (!extra){
        extra = "";
    }
    
    [[SayKitEvent sharedInstance] trackFirebaseEvent:[NSString stringWithUTF8String:event]
                                               extra:[NSString stringWithUTF8String:extra]];
}

void sayKitSetCrashlyticsParam(
                              const char* paramName,
                              const char* paramValue)
{
    [[SayKitEvent sharedInstance] setCrashlyticsParam:[NSString stringWithUTF8String:paramName]
                                           paramValue:[NSString stringWithUTF8String:paramValue]];
}

void sayKitEventTrackFirebaseWithValue(
                              const char* event,
                              const float extra)
{
    if(strcmp(event, strdup("ads_earnings")) == 0)
    {
        [[SayKitEvent sharedInstance] trackFirebaseEventWithValue:[NSString stringWithUTF8String:event] value:extra addCurrency:true];
    }
    else{
        [[SayKitEvent sharedInstance] trackFirebaseEventWithValue:[NSString stringWithUTF8String:event] value:extra addCurrency:false];
    }
}


void sayKitEventTrackFullFirebase(
                              const char* logEvent,
                              const float valueToSum,
                              const char* customJSPN)
{
    [[SayKitEvent sharedInstance] trackFullFirebaseEvent:[NSString stringWithUTF8String:logEvent]
                                              valueToSum:valueToSum
                                                dictJSPN:[NSString stringWithUTF8String:customJSPN]];
}



void sayKitEventTrackFacebook(
                              const char* event,
                              const char* extra)
{
    if (!extra){
        extra = "";
    }
    
    [[SayKitEvent sharedInstance] trackFacebookEvent:[NSString stringWithUTF8String:event]
                                               extra:[NSString stringWithUTF8String:extra]];
}

void sayKitEventTrackFacebookPurchaseEvent(
                                           const float valueToSum,
                                           const char* currencyCode)
{
    if (!currencyCode){
        currencyCode = "";
    }
    
    [[SayKitEvent sharedInstance] trackFacebookPurchaseEvent:valueToSum
                                                currencyCode:[NSString stringWithUTF8String:currencyCode]];
}

void sayKitEventTrackFullFacebook(
                              const char* logEvent,
                              const float valueToSum,
                              const char* customJSPN)
{
    [[SayKitEvent sharedInstance] trackFullFacebookEvent:[NSString stringWithUTF8String:logEvent]
                                              valueToSum:valueToSum
                                                dictJSPN:[NSString stringWithUTF8String:customJSPN]];
}


void sayKitInitTenjin(const char* apiKey) {
    NSString *_apiKey = [NSString stringWithUTF8String:apiKey];

    [TenjinSDK init:_apiKey];
    [TenjinSDK optIn];
    [TenjinSDK connect];
}

void sayKitSendTenjinEvent(const char* eventName) {
    NSString *_eventName = [NSString stringWithUTF8String:eventName];

    [TenjinSDK sendEventWithName:_eventName];
}

void sayKitPingFacebook(const int facebookAutoEventDisabled) {
//    if(facebookAutoEventDisabled == 1){
//        [FBSDKSettings setAutoLogAppEventsEnabled:false];
//    }
    
    [[SayKitEvent sharedInstance] trackFullFacebookEvent:@"fb_mobile_activate_app"
                                              valueToSum:0
                                                dictJSPN:@""];
}

void sayKitShowRateAppPopup() {
    if ([[UIDevice currentDevice].systemVersion floatValue] >= 10.3){
        [SKStoreReviewController requestReview];
    }
}

void sayKitUpdateAmazonMobileAds() {
    NSUserDefaults *userDefaults = NSUserDefaults.standardUserDefaults;
    if (userDefaults != NULL) {
        [userDefaults setObject:@"1" forKey:@"aps_gdpr_pub_pref_li"];
    }
}

const char* sayKitGetSystemLanguage(){

    NSString *language = [[NSLocale preferredLanguages] objectAtIndex:0];
    NSDictionary *languageDic = [NSLocale componentsFromLocaleIdentifier:language];
    NSString *languageCode = [languageDic objectForKey:@"kCFLocaleLanguageCodeKey"];
    
    const char* string = [languageCode UTF8String];
    return string ? strdup(string) : strdup("");
}

const char* sayKitGetIDFA(){
    
    if (@available(iOS 14.5, *))
    {
        NSString *idfaString = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
        
        const char* string = [idfaString UTF8String];
        return string ? strdup(string) : strdup("");
    }
    else if (@available(iOS 14, *))
    {
        NSString *idfaString = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
        
        //before 14.5 ATT flow
        if(idfaString == nil || [idfaString isEqualToString:@"00000000-0000-0000-0000-000000000000"]) {
            [FBAdSettings setAdvertiserTrackingEnabled:false];
            [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:0];
        }
        else {
            [FBAdSettings setAdvertiserTrackingEnabled:true];
            [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:1];
        }
        
        const char* string = [idfaString UTF8String];
        return string ? strdup(string) : strdup("");
    }
    else
    {
        if([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]) {
            NSString *idfaString = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
            
            if(idfaString == nil || [idfaString isEqualToString:@"00000000-0000-0000-0000-000000000000"]) {
                [FBAdSettings setAdvertiserTrackingEnabled:false];
                [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:0];
            }
            else {
                [FBAdSettings setAdvertiserTrackingEnabled:true];
                [[SayKitEvent sharedInstance] setFacebookAdvertiserTrackingEnabledFlag:1];
            }
            
            const char* string = [idfaString UTF8String];
            return string ? strdup(string) : strdup("");
        }
    }
    
    return strdup("");
}



float sayKitBottomSafePadding() {
    if (@available(iOS 11, *)) {
        return (float) [UIApplication sharedApplication].keyWindow.safeAreaInsets.bottom;
    } else {
        return 0;
    }
}

float sayKitScreenScale() {
    return (float) [[UIScreen mainScreen] nativeScale];
}


void sayKitLogSetFlag(
        const int debugFlag)
{
    [[SayKitLog sharedInstance] SetFlag:debugFlag];
}


int getMemoryInfo()
{
    @try {
        
        mach_port_t host_port;
        mach_msg_type_number_t host_size;
        vm_size_t pagesize;
        
        host_port = mach_host_self();
        host_size = sizeof(vm_statistics_data_t) / sizeof(integer_t);
        host_page_size(host_port, &pagesize);
        
        vm_statistics_data_t vm_stat;
        
        if (host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size) != KERN_SUCCESS) {
            NSLog(@"Failed to fetch vm statistics");
        }
        
        int mem_free = (vm_stat.free_count * pagesize)/1048576;
        
        return mem_free;

    } @catch (NSException *exception) {
        NSLog(@"sayKitTrackAvailableMemory error: %@\n", exception.reason);
    }
    
    return 0;
}

void sayKitEventTrackTotalMemory(){
    
    @try {
        int totalMemory = [NSProcessInfo processInfo].physicalMemory/1048576;
        
        [SayKitEvent.sharedInstance trackImmediately:@"total_memory"
                                   param1:totalMemory
                                   param2:0
                                    extra:@""];
        
    } @catch (NSException *exception) {
        NSLog(@"sayKitTrackAvailableMemory error: %@\n", exception.reason);
    }
}


void sayKitEventTrackAvailableMemory(){
    
    int mem_free = getMemoryInfo();
    
    [SayKitEvent.sharedInstance track:@"free_memory"
                               param1:mem_free
                               param2:0
                                extra:@""
                               param3:0
                               param4:0
                               extra2:@""
                                  tag:@""];
}



const char* sayKitGetCurrentIDFAStatus()
{
    SayKitIDFAManager* sayKitIDFAManager = [[SayKitIDFAManager alloc] init];
    NSString *status = [sayKitIDFAManager getTrackingAuthorizationStatus];
    
    const char* string = [status UTF8String];
    return string ? strdup(string) : strdup("");
}

void sayKitShowSystemIDFAPopup(){
    SayKitIDFAManager* sayKitIDFAManager = [[SayKitIDFAManager alloc] init];
    [sayKitIDFAManager showSystemIDFAPopup];
}

void sayKitShowNativeIDFAPopup(const char* title,
                               const char* description,
                               const char* okBtnString,
                               const char* cancelBtnString){
    
    SayKitIDFAManager* sayKitIDFAManager = [[SayKitIDFAManager alloc] init];
    [sayKitIDFAManager showNativeIDFAPopup:[NSString stringWithUTF8String:title]
                               description:[NSString stringWithUTF8String:description]
                               okBtnString:[NSString stringWithUTF8String:okBtnString]
                           cancelBtnString:[NSString stringWithUTF8String:cancelBtnString]];
}

int saykitGetApplicationStartTimestamp()
{
    return [SayKitBridje sharedManager].ApplicationStartTimestamp;
}

void saykitUpdateConversionValue(const int value){
    SayKitIDFAManager* sayKitIDFAManager = [[SayKitIDFAManager alloc] init];
    [sayKitIDFAManager updateConversionValue:value];
}

void sayKitTrackSDKVersions()
{
    [[SayKitVersionManager sharedInstance] trackAdNetworkVersions];
}

void sayKitTrackFirebaseId()
{
    @try {
        [SayKitEvent.sharedInstance track:@"firebase_client_id"
                                   param1:0
                                   param2:0
                                    extra:[FIRAnalytics appInstanceID]
                                   param3:0
                                   param4:0
                                   extra2:@""
                                      tag:@""];
    } @catch (NSException *exception) {
        NSLog(@"sayKitTrackFirebaseId error: %@\n", exception.reason);
    }
}

void sayKitOpenStoreProductView(int store_id, const char* skadData)
{
    NSString* skadJson =  [NSString stringWithUTF8String:skadData];
    NSDictionary *parameters = [SPClientProperties getSKStoreProductParameters:skadJson iTunesItemIdentifier:store_id];
    
    SKStoreProductViewController *storeViewController = [[SKStoreProductViewController alloc] init];
    [storeViewController loadProductWithParameters:parameters
                                   completionBlock:^(BOOL result, NSError *error) {
        if (!result)
        {
            NSLog(@"SKStoreProductViewController: %@", error);
        }
    }];

    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:storeViewController animated:YES completion:nil];
}
