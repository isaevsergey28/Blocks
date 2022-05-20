@import Foundation;

@interface SPWebRequest : NSObject

@property (nonatomic, strong) NSString *url;
@property (nonatomic, strong) NSString *requestType;
@property (nonatomic, strong) NSString *body;
@property (nonatomic, strong) NSDictionary<NSString*,NSArray*> *headers;
@property (nonatomic, strong) NSMutableData *receivedData;
@property (nonatomic, strong) NSError *error;

@property (nonatomic, assign) BOOL canceled;
@property (nonatomic, assign) BOOL timeoutException;
@property (nonatomic, assign) BOOL finished;

@property (nonatomic, strong) NSCondition *blockCondition;

@property (nonatomic, strong) NSMutableURLRequest *request;
@property (nonatomic, assign) int connectTimeout;


- (instancetype)initWithUrl:(NSString *)url requestType:(NSString *)requestType headers:(NSDictionary<NSString*,NSArray<NSString*>*> *)headers connectTimeout:(int)connectTimeout;

- (NSData *)makeRequest;
- (NSData *)makeMockRequest;
- (void)cancel;

-(void) downloadSuccessful;
-(void) downloadFailed:(NSError *)error;

@end
