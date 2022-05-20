#import <UIKit/UIKit.h>

bool sayPromoIsAppInstalled(const char* scheme) {
    NSString *_scheme = [NSString stringWithUTF8String:scheme];
    UIApplication *application = [UIApplication sharedApplication];
    NSURL *URL = [NSURL URLWithString:_scheme];

    return [application canOpenURL:URL];
}
