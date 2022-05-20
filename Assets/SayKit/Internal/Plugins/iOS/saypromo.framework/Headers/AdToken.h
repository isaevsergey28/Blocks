//
//  AdToken.h
//  saypromo
//
//  Created by Timur Dularidze on 12/17/20.
//  Copyright © 2020 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SPDevice.h"


NS_ASSUME_NONNULL_BEGIN

@interface AdToken : NSObject

- (NSString *)generateToken:(NSDictionary *)info;

@end

NS_ASSUME_NONNULL_END
