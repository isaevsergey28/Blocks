//
//  NSCompression.h
//  saypromo
//
//  Created by Timur Dularidze on 23.02.21.
//  Copyright © 2021 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>
#include <zlib.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSCompression : NSData

- (NSData *) gzipDecode:(NSData *)gzipData;

@end

NS_ASSUME_NONNULL_END
