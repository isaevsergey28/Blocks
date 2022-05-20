
#import <Foundation/Foundation.h>


@interface SayKitLog : NSObject
{
    
@private
    bool _debugFlag;
}

+ (SayKitLog*)sharedInstance;


- (void) SetFlag:(int)debugFlag;

- (void) Log:(NSString*)message;

@end
