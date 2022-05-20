// Copyright 2004-present Facebook. All Rights Reserved.
//
// You are hereby granted a non-exclusive, worldwide, royalty-free license to use,
// copy, modify, and distribute this software in source code or binary form for use
// in connection with the web services and APIs provided by Facebook.
//
// As with any software that integrates with the Facebook platform, your use of
// this software is subject to the Facebook Developer Principles and Policies
// [http://developers.facebook.com/policy/]. This copyright notice shall be
// included in all copies or substantial portions of the software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#import <Foundation/Foundation.h>
@class FBBKBid;
@protocol FBBKNotifier;

typedef NS_ENUM(NSInteger, FBBKAdBidAuctionType) {
    FBBKAdBidAuctionTypeFirstPrice = 1,
    FBBKAdBidAuctionTypeSecondPrice = 2
};

NS_ASSUME_NONNULL_BEGIN
/*!
 * Represents a bidder able to retrieve bids in real-time from a given demand source.
 */
@protocol FBBKBidder <NSObject>
/*!
 * Unique identifier for a bidder
 */
@property (nonatomic, readonly) NSInteger identifier;
/*!
 * This method will return a name for bidder
 */
@property (nonatomic, readonly, copy) NSString *bidderName;
/*!
 * This method will return the bid received from the server for the given bidder
 *
 * @param complete Block with the bid received from the server for the given bidder.
 */
- (void)retrieveBidComplete:(void (^)(FBBKBid *_Nullable bid))complete;
/*!
 * This method will return the bid received from the server for the given bidder
 *
 * @param complete Block with the bid received from the server for the given bidder.
 * @param failed Block will be invoked in case of error.
 */
- (void)retrieveBidComplete:(nullable void (^)(FBBKBid *bid))complete
                     failed:(nullable void (^)(NSError *_Nullable error))failed;
@end

NS_ASSUME_NONNULL_END
