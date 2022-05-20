//
//  SayEndpoint.h
//  Unity-iPhone
//
//  Created by Nick Satchok on 11/19/19.
//

#ifndef SayEndpoint_h
#define SayEndpoint_h

@protocol ISayEndpointStringRequest
@required
-(NSString*)getBody;
-(int)getOrder;
@end

@protocol ISayEndpointDeliveryStrategy
@required
-(BOOL)canSendNow;
-(BOOL)isGoodData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error;
@end

@protocol ISayEndpointBatching
@required
-(id<ISayEndpointStringRequest>)combine:(NSArray*)batch;
@end

@interface SayEndpointStrictServerResponseDelivery : NSObject<ISayEndpointDeliveryStrategy>
-(instancetype)initWithResponse:(NSString*)response;
-(BOOL)canSendNow;
-(BOOL)isGoodData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error;
@end

@interface SayEndpointRetryOnAllowedErrorDelivery : SayEndpointStrictServerResponseDelivery
-(BOOL)isGoodData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error;
@end

@interface SayEndpointAppendWithNewLineBatching : NSObject<ISayEndpointBatching>
-(id<ISayEndpointStringRequest>)combine:(NSArray*)batch;
@end

@interface SayEndpointBatch : NSObject
@property(readonly, class, nonatomic) SayEndpointAppendWithNewLineBatching* appendWithNewLine;
@end

@interface SayEndpointStringRequest : NSObject<ISayEndpointStringRequest>

-(instancetype)initWithBody:(NSString*)body
                      order:(int)order
                   batching:(NSObject<ISayEndpointBatching>*) batching
                   priority:(BOOL)priority;

-(NSString*) getBody;
-(int) getOrder;
-(void) setOrder:(int) order;
-(NSObject<ISayEndpointBatching>*) getBatching;
-(BOOL) isPriority;
@end

@interface SayEndpoint : NSObject<NSURLSessionDelegate>

-(instancetype)initWithName:(NSString*)name;
-(void)open;

-(void)setUrl:(NSURL*) url;
-(void)setContentType:(NSString*) contentType;
-(void)setDeliveryStrategy:(id<ISayEndpointDeliveryStrategy>) strategy;
-(void)disableAutoRetry;
-(void)setAutoRetryMillis:(int) millis;
-(void)setMaxBatchSize:(int) size;
-(void)setMaxRequestsCount:(int) count;
-(void)autoFlush:(int) interval;
-(void)addSslPin:(NSDictionary*)pin forDomain:(NSString*)domain;

-(void)addRequest:(NSString*)body;
-(void)addRequest:(NSString*)body withPriority:(BOOL)priority;
-(void)addRequest:(NSString*)body
     withBatching:(NSObject<ISayEndpointBatching>*)batching;
-(void)addRequest:(NSString*)body
     withBatching:(NSObject<ISayEndpointBatching>*)batching
     withPriority:(BOOL)priority;

-(void)sendRequest:(NSString*)body;
-(void)sendRequest:(NSString*)body withPriority:(BOOL)priority;
-(void)sendRequest:(NSString*)body
      withBatching:(NSObject<ISayEndpointBatching>*)batching;
-(void)sendRequest:(NSString*)body
      withBatching:(NSObject<ISayEndpointBatching>*)batching
      withPriority:(BOOL)priority;

-(void)flushRequests;

@end

#endif /* SayEndpoint_h */
