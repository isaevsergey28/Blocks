//
//  AdData.h
//  saypromo
//
//  Created by Timur Dularidze on 4/4/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface AdData : NSObject


@property NSString *requestId;
@property NSString *status;
@property NSString *creativeId;

@property NSMutableArray *creatives;

@property NSString *trackLoadUrl;
@property NSString *trackImpressionUrl;
@property NSString *trackClickUrl;
@property NSString *trackCloseUrl;
@property NSString *trackErrorUrl;
@property NSString *resultUrl;
@property NSString *storeId;

@property BOOL debug;

- (void)readAdDataFromJson:(NSString *)JSONString;

@end

NS_ASSUME_NONNULL_END
