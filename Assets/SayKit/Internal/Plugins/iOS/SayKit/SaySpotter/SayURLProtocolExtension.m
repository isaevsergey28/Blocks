//
//  SayURLProtocolExtension.m
//  Unity-iPhone
//
//  Created by Timur Dularidze on 10/2/20.
//

#import "SayURLProtocolExtension.h"
#import <WebKit/WebKit.h>


Class WK_ContextControllerClass() {
  static Class cls;
  if (!cls) {
    cls = [[[WKWebView new] valueForKey:@"browsingContextController"] class];
  }
  return cls;
}

SEL WK_RegisterSchemeSelector() {
  return NSSelectorFromString(@"registerSchemeForCustomProtocol:");
}

SEL WK_UnregisterSchemeSelector() {
  return NSSelectorFromString(@"unregisterSchemeForCustomProtocol:");
}

@implementation NSURLProtocol (WKWebViewSupport)

+ (void)wk_registerScheme:(NSString *)scheme {
  Class cls = WK_ContextControllerClass();
  SEL sel = WK_RegisterSchemeSelector();
  if ([(id)cls respondsToSelector:sel]) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    [(id)cls performSelector:sel withObject:scheme];
#pragma clang diagnostic pop
  }
}

+ (void)wk_unregisterScheme:(NSString *)scheme {
  Class cls = WK_ContextControllerClass();
  SEL sel = WK_UnregisterSchemeSelector();
  if ([(id)cls respondsToSelector:sel]) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    [(id)cls performSelector:sel withObject:scheme];
#pragma clang diagnostic pop
  }
}

@end

@implementation SayURLProtocolExtension

@end
