//
//  SayEndpointCache.h
//  Unity-iPhone
//
//  Created by Nick Satchok on 11/19/19.
//

#ifndef SayEndpointCache_h
#define SayEndpointCache_h

@class SayEndpointStringRequest;

@interface SayEndpointCache : NSObject
-(instancetype)initWithName:(NSString*) name;
-(void)dealloc;
-(NSArray*)open;
-(void)cacheReques:(SayEndpointStringRequest*) data;
-(void)removeRequestsLessOrEqual:(int) order;
-(void)removeRequest:(SayEndpointStringRequest*) data;
@end

#endif /* SayEndpointCache_h */
