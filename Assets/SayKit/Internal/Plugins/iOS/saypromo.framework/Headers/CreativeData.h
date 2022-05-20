//
//  CreativeData.h
//  saypromo
//
//  Created by Timur Dularidze on 4/5/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface CreativeData : NSObject

@property NSString *type;
@property NSString *url;
@property NSInteger length;
@property NSInteger skipAfter;
@property BOOL showTimer;
@property NSString *closePosition;
@property NSString *sayPosition;
@property NSInteger clickableAfter;
@property BOOL showProgressBar;

@property float height;
@property float width;

@end

NS_ASSUME_NONNULL_END
