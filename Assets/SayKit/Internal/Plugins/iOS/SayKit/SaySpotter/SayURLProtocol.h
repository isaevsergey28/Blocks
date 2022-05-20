//
//  SayURLProtocol.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 9/25/20.
//

#import <Foundation/Foundation.h>
#import "SayEndpoint.h"
#import "SaySslPinner.h"

#import "SayKitLog.h"

NS_ASSUME_NONNULL_BEGIN

@class SayEndpoint;

@interface SayURLProtocol : NSURLProtocol

@end


@interface SayURLProtocolManager: NSObject
{
@private
    SayEndpoint* _endpoint;
    bool _initialized;
    bool _enabled;
    
    bool _configRequested;
    
    
    NSString* _appKey;
    NSString* _idfa;
    NSString* _device_id;
    NSString* _device_os;
    NSString* _device_name;
    NSString* _app_version;
    
    NSTimeInterval _sessionUpdatedAt;
    NSString* _sessionId;
    
    NSString* _bundle;
    NSString* _idfv;
    
    NSMutableArray<NSRegularExpression *> *_blackListRegulars;
    NSMutableArray<NSRegularExpression *> *_whiteListRegulars;
    
}

+ (SayURLProtocolManager*)sharedInstance;
- (SayURLProtocolManager*)init;

- (void) updateEventParameters:(NSString*)appKey
                          idfa:(NSString*)idfa
                     device_os:(NSString*)device_os
                   device_name:(NSString*)device_name;

- (void) updateSessionId:(NSString*)sessionId;

- (void) checkRequest:(NSURLRequest *)request;
- (void) track:(NSString *)absoluteURL HTTPMethod:(NSString*)HTTPMethod;
- (void) downloadConfig;

- (bool) needToTrackURL:(NSString *) url;
- (NSDictionary *) convertDictToJSON:(NSDictionary *) dict;

@end


NS_ASSUME_NONNULL_END
