//
//  SKAdData.h
//  saypromo
//
//  Created by Timur Dularidze on 25.06.21.
//  Copyright © 2021 Timur Dularidze. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface SKAdData : NSData

@property NSString *version;
@property NSString *ad_network_id;
@property NSInteger campaign_id;
@property NSInteger itunes_item_id;

@property NSString *nonce;
@property NSInteger source_app_id;
@property NSInteger fidelity_type;

@property NSInteger timestamp;
@property NSString *signature;

- (void)readAdDataFromJson:(NSString *)JSONString;

@end

NS_ASSUME_NONNULL_END
