//
//  SPDebugLog.h
//  saypromo
//
//  Created by Timur Dularidze on 8/20/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPWebRequest.h"
#import "SPDebugLog.h"
#import "SPDevice.h"

NS_ASSUME_NONNULL_BEGIN


static NSString *const DebugURL =@"https://api.saypromo.net/ad/debug";
// @"http://devapi.saypromo.net:8080/ad/debug";//

@interface SPDebugLog : NSObject

+ (SPDebugLog *)sharedInstance;

@property BOOL DebugMode;
@property NSMutableDictionary<NSString *, NSNumber *> *loggers;
@property NSString *_lastDebugId;

- (NSString *) newDebugLogger;

- (void) trackEventWithId:(NSString *)Id
                   appKey:(NSString *)appKey
                    place:(NSString *)place
                    event:(NSString *)event
                     str1:(NSString *)str1
                     int1:(long)int1
                     int2:(long)int2
                     int3:(long)int3;

-(NSString *) getURL:(NSString *)appKey
               place:(NSString *)place;

@end

NS_ASSUME_NONNULL_END
