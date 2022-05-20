
#import "SayKitLog.h"

@implementation SayKitLog

#pragma mark NSObject

// Manager to be used for methods that do not require a specific adunit to operate on.
+ (SayKitLog*)sharedInstance
{
    static SayKitLog* sharedSayKitLog = nil;
    
    if (!sharedSayKitLog)
    {
        sharedSayKitLog = [SayKitLog alloc];
    }
    return sharedSayKitLog;
}


- (void) SetFlag:(int)debugFlag
{
    if(debugFlag == 0)
    {
        _debugFlag = false;
    }
    else{
        _debugFlag = true;
    }
}

- (void) Log:(NSString*)message
{
    if(_debugFlag){
        NSLog(@"%@", message);
    }
}


@end
