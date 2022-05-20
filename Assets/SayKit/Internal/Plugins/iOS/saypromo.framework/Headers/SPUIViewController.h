//
//  SPUIViewController.h
//  saypromo
//
//  Created by Timur Dularidze on 4/16/19.
//  Copyright © 2019 Timur Dularidze. All rights reserved.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@protocol SPViewControllerDelegate <NSObject>

- (void)onDisappear;

- (void)onAppear;

@end

@interface SPUIViewController : UIViewController

@property (nonatomic, weak) id<SPViewControllerDelegate> delegate;

@end

NS_ASSUME_NONNULL_END
