//
//  SayURLProtocolExtension.h
//  Unity-iPhone
//
//  Created by Timur Dularidze on 10/2/20.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN


@interface NSURLProtocol (WKWebViewSupport)

+ (void)wk_registerScheme:(NSString *)scheme;
+ (void)wk_unregisterScheme:(NSString *)scheme;

@end


@interface SayURLProtocolExtension : NSObject

@end

NS_ASSUME_NONNULL_END
