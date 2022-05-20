#import "SayEndpoint.h"
#import "SayEndpointCache.h"
#import "SaySslPinner.h"
#import "SayKitEvent.h"

#include <thread>
#include <vector>

static constexpr long SSL_PINNING_ERROR_CODE = -999;
static constexpr long OFFLINE_ERROR_CODE = -1009;

static inline dispatch_queue_global_t getDispatchQueue()
{
    return dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0);
}

static void crashlyticsLog(NSString* msg, int code)
{
    NSError* error = [NSError errorWithDomain:@"SayEndpoint" code:code userInfo:@{
        NSLocalizedDescriptionKey: msg
    }];
    [[FIRCrashlytics crashlytics] recordError:error];
}

static void crashlyticsLog(NSString* msg, NSException* e, int code)
{
    NSError* error = [NSError errorWithDomain:@"SayEndpoint" code:code userInfo:@{
        NSLocalizedDescriptionKey: msg,
        NSLocalizedFailureReasonErrorKey: e.reason
    }];
    [[FIRCrashlytics crashlytics] recordError:error];
}

@interface StringRequestQueue : NSObject
-(void)setMaxRequestsCount:(int) count;
-(SayEndpointStringRequest*)addRequest:(SayEndpointStringRequest*)request;
-(void)addObjectsFromArray:(NSArray*)array;
-(int)count;
-(NSArray*) getRequests:(int)maxCount;
-(int)getLatestRequestOrder;
-(void)removeRequestsLessOrEqual:(int)order;
-(void)clear;
@end

static bool isSameBatching(NSObject<ISayEndpointBatching>* lhs, NSObject<ISayEndpointBatching>* rhs)
{
    return lhs == rhs || (lhs != NULL && rhs != NULL && [lhs isMemberOfClass:[rhs class]]);
}

@interface SayEndpoint()
@property (atomic) SaySslPinner* pinner;
@end

@implementation SayEndpoint
{
    StringRequestQueue* _queue;
    SayEndpointCache* _cache;
    NSURLSession* _pinnedSession;
    NSMutableDictionary* _pinConfig;
    id<ISayEndpointStringRequest> _pendingRequest;
    NSString* _name;
    BOOL _isOpened;
    NSURL* _url;
    id<ISayEndpointDeliveryStrategy> _delivery;
    NSString* _contentType;
    BOOL _isRetryDisabled;
    uint64_t _retryDelay;
    int _maxBatchSize;
    BOOL _isAutoFlushing;
}

-(instancetype)initWithName:(NSString*)name
{
    if (self = [self init])
    {
        _name = [name copy];
        _queue = [[StringRequestQueue alloc] init];
        _cache = NULL;
        _pendingRequest = NULL;
        _isOpened = NO;
        _url = NULL;
        _delivery = [[SayEndpointRetryOnAllowedErrorDelivery alloc] initWithResponse:@"ok"];
        _contentType = @"application/json; charset=utf-8";
        _isRetryDisabled = NO;
        _retryDelay = 5000 * NSEC_PER_MSEC;
        _maxBatchSize = 100;
        _isAutoFlushing = NO;
        _pinConfig = [NSMutableDictionary new];
        _pinner = [[SaySslPinner alloc] initWithConfig:_pinConfig];
        
        NSURLSessionConfiguration* sessionConfig = [NSURLSessionConfiguration defaultSessionConfiguration];
        _pinnedSession = [NSURLSession sessionWithConfiguration:sessionConfig delegate:self delegateQueue:nil];
    }
    
    return self;
}

-(void)open
{
    @synchronized (self) {
        
        if (_cache != NULL)
        {
            return;
        }
        
        _cache = [[SayEndpointCache alloc] initWithName:_name];
        
        try {
            std::thread thread([=]{
                NSArray* savedRequests = [_cache open];
                [self onCacheOpened:savedRequests];
            });
            thread.detach();
        } catch (...) {
            _isOpened = YES;
            crashlyticsLog(@"Can't start SayEndpoint cache openning thread", 1);
        }
    }
}

-(void)setUrl:(NSURL*) url
{
    @synchronized (self) {
        _url = [url copy];
    }
}

-(void)setContentType:(NSString*) contentType
{
    @synchronized (self) {
        _contentType = [contentType copy];
    }
}

-(void)setDeliveryStrategy:(id<ISayEndpointDeliveryStrategy>) strategy
{
    @synchronized (self) {
        _delivery = strategy;
    }
}

-(void)setMaxRequestsCount:(int) count
{
    @synchronized (self) {
        [_queue setMaxRequestsCount:count];
    }
}

-(void)disableAutoRetry
{
    @synchronized (self) {
        _isRetryDisabled = YES;
    }
}

-(void)setAutoRetryMillis:(int) millis
{
    @synchronized (self) {
        _retryDelay = millis * NSEC_PER_MSEC;
    }
}

-(void)setMaxBatchSize:(int) size
{
    @synchronized (self) {
        _maxBatchSize = size;
    }
}

-(void)autoFlush:(int) interval
{
    @synchronized (self)
    {
        if (_isAutoFlushing)
        {
            return;
        }
        
        _isRetryDisabled = YES;
        _isAutoFlushing = YES;
        
        __block __weak void (^weak_routine)();
        void (^routine)();
        weak_routine = routine = ^{
            [self flushRequests];
            dispatch_after(dispatch_time(DISPATCH_TIME_NOW, interval * NSEC_PER_MSEC),
                getDispatchQueue(),
                weak_routine);
        };
        
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, 0),
            getDispatchQueue(),
            routine);
    }
}

-(void)addSslPin:(NSDictionary*)pin forDomain:(NSString*)domain
{
    @synchronized (_pinConfig) {
        _pinConfig[domain] = SaySslPinnerConfig(pin);
        self.pinner = [[SaySslPinner alloc] initWithConfig:_pinConfig];
    }
}

-(void)addRequest:(NSString*)body
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:NULL withPriority:NO];
    }
}

-(void)addRequest:(NSString*)body withPriority:(BOOL)priority
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:NULL withPriority:priority];
    }
}

-(void)addRequest:(NSString*)body withBatching:(NSObject<ISayEndpointBatching>*)batching
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:batching withPriority:NO];
    }
}

-(void)addRequest:(NSString*)body
     withBatching:(NSObject<ISayEndpointBatching>*)batching
     withPriority:(BOOL)priority
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:batching withPriority:priority];
    }
}

-(void)sendRequest:(NSString*)body;
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:NULL withPriority:NO];
        [self doFlushRequests];
    }
}

-(void)sendRequest:(NSString*)body withPriority:(BOOL)priority
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:NULL withPriority:priority];
        [self doFlushRequests];
    }
}

-(void)sendRequest:(NSString*)body withBatching:(NSObject<ISayEndpointBatching>*)batching
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:batching withPriority:NO];
        [self doFlushRequests];
    }
}

-(void)sendRequest:(NSString*)body
      withBatching:(NSObject<ISayEndpointBatching>*)batching
      withPriority:(BOOL)priority
{
    @synchronized (self)
    {
        [self doAddRequest:body withBatching:batching withPriority:priority];
        [self doFlushRequests];
    }
}

-(void)flushRequests
{
    @synchronized (self)
    {
        [self doFlushRequests];
    }
}

-(void)onCacheOpened:(NSArray*)savedRequests
{
    @synchronized (self) {
        _isOpened = YES;
        
        @try {
            NSArray* pendingQueue = [_queue getRequests:[_queue count]];
            [_queue clear];
            [_queue addObjectsFromArray:savedRequests];
            
            int baseOrder = [_queue getLatestRequestOrder];
            for (int i = 0; i < [pendingQueue count]; ++i) {
                SayEndpointStringRequest* request = pendingQueue[i];
                [request setOrder:baseOrder + i + 1];
                [_cache cacheReques:request];
            }
            
            [self doFlushRequests];
            
            [_queue addObjectsFromArray:pendingQueue];
            
        } @catch (NSException *exception) {
            crashlyticsLog(@"Error while opening endpoint cache", exception, 3);
        }
    }
}

-(SayEndpointStringRequest*)doAddRequest:(NSString*)body
       withBatching:(NSObject<ISayEndpointBatching>*)batching
       withPriority:(BOOL)priority
{
    SayEndpointStringRequest* request = [[SayEndpointStringRequest alloc]
                                         initWithBody: body
                                         order:[_queue getLatestRequestOrder]+1
                                         batching:batching
                                         priority:priority];
    SayEndpointStringRequest* removed = [_queue addRequest: request];
    if (removed != request)
    {
        [self tryCacheRequest: request];
    }
    if (removed != NULL)
    {
        [self tryRemoveRequest: request];
    }
    return request;
}

-(void)tryCacheRequest:(SayEndpointStringRequest*)request
{
    if (!_isOpened)
    {
        return;
    }
    
    [_cache cacheReques:request];
}

-(void)tryRemoveRequest:(SayEndpointStringRequest*)request
{
    if (!_isOpened)
    {
        return;
    }
    
    [_cache removeRequest:request];
}

-(id<ISayEndpointStringRequest>)createPendingRequest
{
    NSArray* queue = [_queue getRequests:_maxBatchSize];
    
    if ([queue count] == 0)
    {
        return NULL;
    }
    
    SayEndpointStringRequest* request = queue[0];
    NSObject<ISayEndpointBatching>* batching = [request getBatching];
    if (batching == NULL)
    {
        return request;
    }
    
    NSMutableArray* batch = [NSMutableArray new];
    [batch addObject:request];
    for (int i = 1; i < [queue count]; ++i)
    {
        SayEndpointStringRequest* cur = queue[i];
        if (isSameBatching(batching, [cur getBatching]))
        {
            [batch addObject:cur];
        }
        else
        {
            break;
        }
    }
    
    if ([batch count] == 1)
    {
        return batch[0];
    }
    else
    {
        return [batching combine:batch];
    }
}

-(void)removeRequestsLessOrEqual:(int) order
{
    [_queue removeRequestsLessOrEqual:order];
    [_cache removeRequestsLessOrEqual:order];
}

-(void) doFlushRequests
{
    if (_pendingRequest != NULL)
    {
        NSLog(@"Skipping flush due to pending request");
        return;
    }
    
    if (!_isOpened)
    {
        NSLog(@"Skipping flush. Endpoint cache is not opened");
        [self tryScheduleRetry];
        return;
    }
    
    if (_url == NULL)
    {
        NSLog(@"Skipping flush due to url is not set");
        [self tryScheduleRetry];
        return;
    }
    
    if (![_delivery canSendNow])
    {
        NSLog(@"Skipping flush due to _delivery forbids sending");
        [self tryScheduleRetry];
        return;
    }
    
    @try {
        _pendingRequest = [self createPendingRequest];
    } @catch (NSException *exception) {
        crashlyticsLog(@"Can't create endpoint request", exception, 4);
    }
    
    
    if (_pendingRequest == NULL)
    {
        NSLog(@"Nothing to send");
        return;
    }
    
    @try
    {
        NSString* bufferToSend = [_pendingRequest getBody];
        
        NSData *postData = [bufferToSend dataUsingEncoding:NSASCIIStringEncoding allowLossyConversion:YES];
        NSString *postLength = [NSString stringWithFormat:@"%lu", (unsigned long)[postData length]];
        
        NSMutableURLRequest *request = [[NSMutableURLRequest alloc] init];
        [request setURL:_url];
        [request setHTTPMethod:@"POST"];
        [request setValue:postLength forHTTPHeaderField:@"Content-Length"];
        [request setValue:_contentType forHTTPHeaderField:@"Content-Type"];
        [request setHTTPBody:postData];
        
        NSURLSessionDataTask *task = [_pinnedSession
                                      dataTaskWithRequest:request
                                      completionHandler:
                                      ^(NSData * _Nullable data,
                                        NSURLResponse * _Nullable response,
                                        NSError * _Nullable error)
                                    {
                                        [self handleCompletionData:data response:response error:error];
                                    }];
        [task resume];
        
        NSLog(@"sending events size = %lu\n", (unsigned long)[bufferToSend length]);
    }
    @catch(NSException* e)
    {
        [self handleFailure];
        crashlyticsLog(@"Error while sending request", e, 5);
    }
}

- (void)URLSession:(NSURLSession *)session didReceiveChallenge:(NSURLAuthenticationChallenge *)challenge
    completionHandler:(void (^)(NSURLSessionAuthChallengeDisposition disposition, NSURLCredential * _Nullable credential))completionHandler
{
    @try
    {
        if ([self.pinner handleChallenge:challenge completionHandler:completionHandler])
        {
            return;
        }
    }
    @catch (NSException *exception)
    {
        crashlyticsLog(@"Error in ssl pinning", exception, 20);
    }
    
    completionHandler(NSURLSessionAuthChallengePerformDefaultHandling, nil);
}

-(void)tryScheduleRetry
{
    if (_isRetryDisabled)
    {
        return;
    }
    
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, _retryDelay),
            getDispatchQueue(),
            ^(void)
            {
                [self flushRequests];
            });
}

-(void)handleCompletionData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error
{
    @synchronized (self) {
        if ([_delivery isGoodData:data response:response error:error])
        {
            [self finishPendingRequest];
        }
        else
        {
            NSLog(@"Sayendpoint request failed:\n%@\n%@\n%@", data, response, error);
            [self handleFailure];
        }
    }
}

-(void)finishPendingRequest
{
    @try {
        int sentOrder = [_pendingRequest getOrder];
        [self removeRequestsLessOrEqual: sentOrder];
    } @catch (NSException *exception) {
        crashlyticsLog(@"Error while finishing pending request", exception, 6);
    }
    
    _pendingRequest = NULL;
    [self doFlushRequests];
}

-(void)handleFailure
{
    _pendingRequest = NULL;
    [self tryScheduleRetry];
}

@end

@implementation SayEndpointBatch

static SayEndpointAppendWithNewLineBatching* const _appendWithNewLine = [[SayEndpointAppendWithNewLineBatching alloc] init];
+(SayEndpointAppendWithNewLineBatching*)appendWithNewLine
{
    return _appendWithNewLine;
}

@end

@implementation SayEndpointStringRequest
{
    NSString* _body;
    int _order;
    NSObject<ISayEndpointBatching>* _batching;
    BOOL _priority;
    BOOL _pinned;
}

-(instancetype)initWithBody:(NSString*)body
                      order:(int)order
                   batching:(NSObject<ISayEndpointBatching>*) batching
                   priority:(BOOL)priority
{
    if (self = [self init])
    {
        _body = body;
        _order = order;
        _batching = batching;
        _priority = priority;
        _pinned = NO;
    }
    
    return self;
}

-(NSString*) getBody
{
    return _body;
}

-(int) getOrder
{
    return _order;
}
-(void) setOrder:(int) order
{
    _order = order;
}

-(NSObject<ISayEndpointBatching>*) getBatching
{
    return _batching;
}

-(BOOL) isPriority
{
    return _priority;
}
@end

@implementation SayEndpointStrictServerResponseDelivery
{
    NSString* _response;
}
-(instancetype)initWithResponse:(NSString*)response
{
    if (self = [self init])
    {
        _response = [response copy];
    }
    
    return self;
}

-(BOOL)canSendNow
{
    return YES;
}

-(BOOL)isGoodData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error
{
    if (error != NULL)
    {
        return NO;
    }
    //TODO check response?
    if (data != NULL)
    {
        NSString* serverResponse = [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
        return _response && [_response caseInsensitiveCompare:serverResponse] == NSOrderedSame;
    }
    
    return NO;
}

@end

static NSDictionary* g_allowedErrors = @{
    NSURLErrorDomain: [NSSet setWithObjects:@-999, @-1003, @-1009, @-1018, @-1202, nil],
    NSPOSIXErrorDomain: [NSSet setWithObjects:@9, nil],
    (id)kCFErrorDomainCFNetwork: [NSSet setWithObjects:@310, @311, nil]
};

@implementation SayEndpointRetryOnAllowedErrorDelivery
-(BOOL)isGoodData:(NSData*)data response:(NSURLResponse*)response error:(NSError*)error
{
    if ([super isGoodData:data response:response error:error])
    {
        return YES;
    }
    else if (error != nil)
    {
        NSSet* codeSet = [g_allowedErrors objectForKey:error.domain];
        if (codeSet != nil)
        {
            return ![codeSet containsObject:@(error.code)];
        }
        else
        {
            return YES;
        }
    }
    else if (response != nil)
    {
        NSHTTPURLResponse* httpResponse = (NSHTTPURLResponse*)response;
        if (httpResponse.statusCode == 408) //skip Request Timeout errors
        {
            return YES;
        }
    }
    return NO;
}
@end

@interface BatchStringRequest : NSObject<ISayEndpointStringRequest>
-(instancetype)initWithBody:(NSString*)body batch:(NSArray*)batch;
-(NSString*) getBody;
-(int) getOrder;
@end

@implementation SayEndpointAppendWithNewLineBatching

-(id<ISayEndpointStringRequest>)combine:(NSArray*)batch
{
    NSMutableString* body = [NSMutableString stringWithString:@""];
    for (SayEndpointStringRequest* request in batch) {
        if ([body length] > 0)
        {
            [body appendString:@"\n"];
        }
        [body appendString:[request getBody]];
    }
    return [[BatchStringRequest alloc] initWithBody:body batch:batch];
}

@end

@implementation BatchStringRequest
{
    NSString* _body;
    NSArray* _batch;
}

-(instancetype)initWithBody:(NSString*)body batch:(NSArray*)batch
{
    if (self = [self init])
    {
        _body = body;
        _batch = batch;
    }
    
    return self;
}

-(NSString*) getBody
{
    return _body;
}

-(int) getOrder
{
    return [_batch[_batch.count - 1] getOrder];
}

@end

namespace  {
    class CircularBuffer
    {
    private:
        static constexpr int SMALL_BUFFER_SIZE = 100;
        
        std::vector<SayEndpointStringRequest*> _buffer;
        int _first = -1;
        int _last = -1;
        
    public:
        CircularBuffer()
        {
            _buffer.reserve(SMALL_BUFFER_SIZE);
        }
        
        bool isEmpty() const
        {
            return _buffer.empty();
        }
        
        void push(SayEndpointStringRequest* request, int maxRequestsCount)
        {
            if (maxRequestsCount <= 0) {
                crashlyticsLog(@"SayEndpoint: maxRequestsCount <= 0", 2);
                return;
            }

            if (int(_buffer.size()) < maxRequestsCount && _last == _buffer.size() - 1) {
                _buffer.push_back(request);
                _last = int(_buffer.size()) - 1;
                if (_first == -1) {
                    _first = 0;
                }
            } else {

                ++_last;

                if (_last >= _buffer.size()) {
                    _last = 0;
                }

                if (_last == _first) {
                    ++_first;

                    if (_first >= _buffer.size()) {
                        _first = 0;
                    }
                }

                _buffer[_last] = request;

            }
        }
        
        SayEndpointStringRequest* pop()
        {
            SayEndpointStringRequest* popped = doPop();
            optimizeBuffer();
            return popped;
        }
        
        SayEndpointStringRequest* operator[](int index) const
        {
            int effectiveIndex = _first + index;
            if (effectiveIndex >= _buffer.size()) {
                effectiveIndex -= _buffer.size();
            }
            return _buffer[effectiveIndex];
        }
        
        int size() const
        {
            if (_buffer.empty()) {
                return 0;
            } else if (_first <= _last) {
                return _last - _first + 1;
            } else {
                return int(_buffer.size()) - _first + _last + 1;
            }
        }
        
        int getLatestRequestOrder() const
        {
            if (_last == -1) {
                return -1;
            } else {
                return [_buffer[_last] getOrder];
            }
        }
        
        void removeRequestsLessOrEqual(int order)
        {
            if (_first == -1) {
                return;
            }
            while (!isEmpty()) {
                SayEndpointStringRequest* request = _buffer[_first];
                if ([request getOrder] <= order) {
                    doPop();
                } else {
                    break;
                }
            }
            optimizeBuffer();
        }
        
        void clear()
        {
            if (_buffer.empty()) {
                return;
            }

            _buffer.clear();
            if (_buffer.capacity() > SMALL_BUFFER_SIZE)
            {
                _buffer.shrink_to_fit();
                _buffer.reserve(SMALL_BUFFER_SIZE);
            }
            _first = -1;
            _last = -1;
        }
        
    private:
        SayEndpointStringRequest* doPop()
        {
            if (_first == -1) {
                return NULL;
            }

            SayEndpointStringRequest* popped = _buffer[_first];
            _buffer[_first] = NULL;
            moveFirst();
            return popped;
        }
        
        void moveFirst() {
            if (_first == _last) {
                clear();
                return;
            }

            ++_first;

            if (_first >= _buffer.size()) {
                _first = 0;
            }
        }
        
        void optimizeBuffer() {
            if (isEmpty() || (_first == 0 && _last == _buffer.size() - 1) || size() > SMALL_BUFFER_SIZE) {
                return;
            }

            std::vector<SayEndpointStringRequest*> buffer;
            static const int smallBufferSize = SMALL_BUFFER_SIZE; //strange linker bug
            buffer.reserve(std::max(size(), smallBufferSize));
            for (int i = 0; i < size(); ++i) {
                buffer.push_back(operator[](i));
            }
            _buffer = std::move(buffer);
            _first = 0;
            _last = int(_buffer.size()) - 1;
        }
    };
}

@implementation StringRequestQueue
{
    CircularBuffer _buffer;
    CircularBuffer _priorityBuffer;
    int _maxRequestsCount;
}

-(instancetype)init
{
    if (self = [super init])
    {
        _maxRequestsCount = 13000;
    }
    return self;
}
-(void)setMaxRequestsCount:(int) count
{
    _maxRequestsCount = count;
}

-(SayEndpointStringRequest*)addRequest:(SayEndpointStringRequest*)request
{
    SayEndpointStringRequest* poppedRequest = NULL;
    
    if ([self count] >= _maxRequestsCount)
    {
        if (_buffer.isEmpty())
        {
            
            if (![request isPriority])
            {
                return request;
            }
            poppedRequest = _priorityBuffer.pop();
        }
        else
        {
            poppedRequest = _buffer.pop();
        }
    }
    
    if ([request isPriority])
    {
        _priorityBuffer.push(request, _maxRequestsCount);
    }
    else
    {
        _buffer.push(request, _maxRequestsCount);
    }
    
    return poppedRequest;
}

-(void)addObjectsFromArray:(NSArray*)array
{
    for (SayEndpointStringRequest* request in array)
    {
        [self addRequest:request];
    }
}

-(int)count
{
    return _buffer.size() + _priorityBuffer.size();
}

//collect request from two buffers, ordered
-(NSArray*) getRequests:(int)maxCount;
{
    NSMutableArray* array = [NSMutableArray new];
    int fromSimple = 0;
    int fromPriority = 0;
    
    for (int i = 0; i < maxCount; ++i)
    {
        SayEndpointStringRequest* simple = NULL;
        SayEndpointStringRequest* priority = NULL;
        
        if (fromSimple < _buffer.size())
        {
            simple = _buffer[fromSimple];
        }
        if (fromPriority < _priorityBuffer.size())
        {
            priority = _priorityBuffer[fromPriority];
        }
        
        if (simple == NULL && priority == NULL) {
            break;
        } else if (simple == NULL) {
            [array addObject:priority];
            ++fromPriority;
        } else if (priority == NULL) {
            [array addObject:simple];
            ++fromSimple;
        } else if ([simple getOrder] < [priority getOrder]) {
            [array addObject:simple];
            ++fromSimple;
        } else {
            [array addObject:priority];
            ++fromPriority;
        }
    }
    
    return array;
}

-(int)getLatestRequestOrder
{
    return std::max(_buffer.getLatestRequestOrder(), _priorityBuffer.getLatestRequestOrder());
}

-(void)removeRequestsLessOrEqual:(int)order
{
    _buffer.removeRequestsLessOrEqual(order);
    _priorityBuffer.removeRequestsLessOrEqual(order);
}

-(void)clear
{
    _buffer.clear();
    _priorityBuffer.clear();
}

@end
