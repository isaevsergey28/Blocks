//
//  SayURLProtocol.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 9/25/20.
//

#import "SayURLProtocol.h"

@implementation SayURLProtocol

+ (BOOL)canInitWithRequest:(NSURLRequest *)request {
    [[SayURLProtocolManager sharedInstance] checkRequest:request];
    
    return NO;
}

@end


@implementation SayURLProtocolManager

static int *const Version = 2;


+ (SayURLProtocolManager*)sharedInstance
{
    static SayURLProtocolManager* sharedSayURLProtocolManager = nil;
    
    if (!sharedSayURLProtocolManager)
    {
        sharedSayURLProtocolManager = [[SayURLProtocolManager alloc] init];
    }
    return sharedSayURLProtocolManager;
}

- (SayURLProtocolManager*)init
{
    if (self = [super init])
    {
        _endpoint = [[SayEndpoint alloc] initWithName:@"SayURLProtocol"];
        [_endpoint addSslPin:@{
            kSSPPublicKeyHashes: @[@"4a6cPehI7OG6cuDZka5NDZ7FR8a60d3auda+sKfg4Ng="],
            kSSPEnforcePinning: @YES,
            kSSPIncludeSubdomains: @YES
        }
                   forDomain:@"spotter.saygames.io"];
        
        [_endpoint open];
        [_endpoint autoFlush:5 * 1000];
        
        _whiteListRegulars = [[NSMutableArray<NSRegularExpression *> alloc] init];
        _blackListRegulars = [[NSMutableArray<NSRegularExpression *> alloc] init];
        
        _initialized = false;
        
        _bundle = [[NSBundle mainBundle] bundleIdentifier];
        _app_version = [NSString stringWithFormat:@"%@", [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"]];
        _idfv = [[[UIDevice currentDevice] identifierForVendor] UUIDString];
        
    }
    
    return self;
}


- (void) updateEventParameters:(NSString*)appKey
                          idfa:(NSString*)idfa
                     device_os:(NSString*)device_os
                   device_name:(NSString*)device_name;
{
    _appKey = [NSString stringWithString:appKey];
    _idfa = [NSString stringWithString:idfa];
    _device_os = [NSString stringWithString:device_os];
    _device_name = [NSString stringWithString:device_name];
    
    NSString* url = [NSString stringWithFormat:@"https://spotter.saygames.io/spotter/events"];
    [_endpoint setUrl:[NSURL URLWithString:url]];
    
    if(_idfa && _device_os && _device_name && _appKey)
    {
        if(!_configRequested){
            _configRequested = true;
            
            [self downloadConfig];
        }
    }
}

- (void) updateSessionId:(NSString*)sessionId
{
    _sessionId = [NSString stringWithString:sessionId];
}


- (void) checkRequest:(NSURLRequest *)request{
    
    @try
    {
        if(request)
        {
            if(request.URL)
            {
                if(request.HTTPMethod)
                {
                    NSString *urlString = [NSString stringWithString:request.URL.absoluteString];
                    NSString *httpMethod = [NSString stringWithString:request.HTTPMethod];
                    
                    [[SayURLProtocolManager sharedInstance] track:urlString HTTPMethod:httpMethod];
                }
            }
        }
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
    }
    
}

- (void) track:(NSString *)absoluteURL HTTPMethod:(NSString*)HTTPMethod;
{
    if(_enabled == false)
    {
        return;
    }
    
    if(![self needToTrackURL:absoluteURL])
    {
        return;
    }
    
    if(_initialized)
    {
        NSTimeInterval timeStamp = [[NSDate date] timeIntervalSince1970];
        NSNumber *client_ts = [NSNumber numberWithDouble: timeStamp];
        
        @try {
            
            if(_sessionId && _idfa && _idfv && _device_os && _device_name && _appKey && _bundle && _app_version)
            {
                NSMutableDictionary *eventData = [[NSMutableDictionary alloc] initWithCapacity:15];
                [eventData setObject:[NSNumber numberWithLong:client_ts.longValue] forKey:@"client_ts"];
                [eventData setObject:_sessionId forKey:@"session"];
                [eventData setObject:_idfa forKey:@"idfa"];
                [eventData setObject:_idfv forKey:@"idfv"];
                [eventData setObject:_device_os forKey:@"device_os"];
                [eventData setObject:_device_name forKey:@"device_name"];
                
                [eventData setObject:_appKey forKey:@"app_key"];
                [eventData setObject:_bundle forKey:@"bundle"];
                [eventData setObject:_app_version forKey:@"app_version"];
                [eventData setObject:@"ios" forKey:@"platform"];
                
                [eventData setObject:[NSString stringWithString:HTTPMethod] forKey:@"n_method"];
                [eventData setObject:[NSString stringWithString:absoluteURL] forKey:@"url"];
                
                
                NSError *error = nil;
                NSData *jsonData = [NSJSONSerialization dataWithJSONObject:eventData options:0 error:&error];
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                
                // NSLog(@"SaySpotter sent: %@", absoluteURL);
                //                            NSLog(@"SaySpotter: %@", jsonString);
                
                [_endpoint addRequest:jsonString withBatching:SayEndpointBatch.appendWithNewLine withPriority:false];
            }
            else{
                [[SayKitLog sharedInstance] Log:@"SaySpotter: not all params initialized."];
            }
        } @catch (NSException *exception) {
            [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
        }
    }
    
}


- (void) downloadConfig
{
    @try {
        NSString* url = [NSString stringWithFormat:@"https://spotter.saygames.io/spotter/config"];
        
        NSTimeInterval timeStamp = [[NSDate date] timeIntervalSince1970];
        NSNumber *client_ts = [NSNumber numberWithDouble: timeStamp];
        
        NSDictionary *data = NULL;
        
        data = @{
            @"client_ts" : [NSNumber numberWithLong:client_ts.longValue],
            @"idfa" : _idfa,
            @"idfv" : _idfv,
            @"device_os" : _device_os,
            @"device_name" : _device_name,
            
            @"app_key": _appKey,
            @"bundle" : _bundle,
            @"app_version" : _app_version,
            @"platform" : @"ios",
            @"version" : [[NSNumber alloc] initWithInt:Version]
        };
        
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:&error];
        
        NSString *postLength = [NSString stringWithFormat:@"%lu", (unsigned long)[jsonData length]];
        
        NSMutableURLRequest *request = [[NSMutableURLRequest alloc] init];
        [request setURL:[NSURL URLWithString:url]];
        [request setHTTPMethod:@"POST"];
        [request setValue:postLength forHTTPHeaderField:@"Content-Length"];
        [request setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
        [request setHTTPBody:jsonData];
        
        NSURLSession *session = [NSURLSession sharedSession];
        NSURLSessionDataTask *task = [session
                                      dataTaskWithRequest:request
                                      completionHandler:
                                      ^(NSData * _Nullable data,
                                        NSURLResponse * _Nullable response,
                                        NSError * _Nullable error)
                                      {
            
            @try {
                
                if(data != nil)
                {
                    NSError *errorSerialization;
                    NSMutableDictionary * innerJson = [NSJSONSerialization
                                                       JSONObjectWithData:data options:kNilOptions error:&errorSerialization
                                                       ];
                    
                    for (NSString* key in innerJson) {
                        if( [key isEqualToString:@"is_enabled"]) {
                            NSInteger* value = [innerJson[key] integerValue];
                            if((int)value == 1)
                            {
                                _enabled = true;
                            }
                        }
                        
                        if( [key isEqualToString:@"blacklist"]) {
                            id value = innerJson[key];
                            if([value isKindOfClass:[NSArray class]]) {
                                for (NSString* pattern in value) {
                                    NSRegularExpression* regex = [NSRegularExpression regularExpressionWithPattern: pattern options:0 error:&error];
                                    
                                    [_blackListRegulars addObject:regex];
                                }
                            }
                        }
                        else if( [key isEqualToString:@"whitelist"]) {
                            id value = innerJson[key];
                            if([value isKindOfClass:[NSArray class]]) {
                                for (NSString* pattern in value) {
                                    NSRegularExpression* regex = [NSRegularExpression regularExpressionWithPattern: pattern options:0 error:&error];
                                    
                                    [_whiteListRegulars addObject:regex];
                                }
                            }
                        }
                    }
                    
                    _initialized = true;
                }
                
            } @catch (NSException *exception) {
                [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
            }
        }];
        [task resume];
        
    } @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
    }
}


- (bool) needToTrackURL:(NSString *) url
{
    @try {
        if(url == nil || url.length <=0)
        {
            return false;
        }
        
        if([url containsString:@"spotter.saygames.io"])
        {
            //            NSLog(@"SaySpotter skip url spotter.saygames.io");
            return false;
        }
        
        
        NSRange searchedRange = NSMakeRange(0, [url length]);
        
        if(_blackListRegulars.count > 0)
        {
            // Alternative is NSPredicate
            for(NSRegularExpression* regex in _blackListRegulars)
            {
                NSTextCheckingResult* matched =[regex firstMatchInString:url options:NSMatchingAnchored range:searchedRange];
                if(matched)
                {
                    return false;
                }
            }
        }
        
        if(_whiteListRegulars.count > 0)
        {
            for(NSRegularExpression* regex in _whiteListRegulars)
            {
                NSTextCheckingResult* matched =[regex firstMatchInString:url options:NSMatchingAnchored range:searchedRange];
                if(matched)
                {
                    return true;
                }
            }
            
            return false;
        }
        else{
            return true;
        }
    }
    @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
    }
    
    return false;
}


- (NSDictionary *) convertDictToJSON:(NSDictionary *) dict
{
    @try {
        
        if(dict != nil) {
            
            NSObject *networkIdObject = [dict objectForKey:@"networkId"];
            NSObject *placementIdObject = [dict objectForKey:@"placementId"];
            
            if(networkIdObject && placementIdObject){
                NSInteger* networkId = [[dict objectForKey:@"networkId"] integerValue];
                NSString* placementId = [[dict objectForKey:@"placementId"] stringValue];
                
                NSDictionary* filteredDictionary = @{
                    @"networkId" : [NSNumber numberWithInteger:networkId],
                    @"placementId" : placementId
                };
                
                return  filteredDictionary;
            }
        }
    }
    @catch (NSException *exception) {
        [[SayKitLog sharedInstance] Log:[NSString stringWithFormat:@"spotter error: %@\n", exception.reason]];
    }
    
    return @{};
}

@end




