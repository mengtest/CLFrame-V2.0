//
//  IAPHelper.h
//  Unity-iPhone
//
//  Created by chenbin on 14-3-25.
//
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface IAPHelper : NSObject<SKProductsRequestDelegate,SKPaymentTransactionObserver>
{
	NSString *gameObjectName;;
}

+(IAPHelper *) instance;
-(void) initWithGameObjectName:(NSString*)goName;
- (void) requestProUpgradeProductData;
-(void)RequestProductData:(NSArray*)product;
-(bool)CanMakePay;
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions;
-(void) PurchasedTransaction: (SKPaymentTransaction *)transaction;
- (void) completeTransaction: (SKPaymentTransaction *)transaction;
- (void) failedTransaction: (SKPaymentTransaction *)transaction;
-(void) paymentQueueRestoreCompletedTransactionsFinished: (SKPaymentTransaction *)transaction;
-(void) paymentQueue:(SKPaymentQueue *) paymentQueue restoreCompletedTransactionsFailedWithError:(NSError *)error;
- (void) restoreTransaction: (SKPaymentTransaction *)transaction;
-(void)provideContent:(NSString *)product;
-(void)recordTransaction:(NSString *)product;
-(void) sendUnityMessage:(const NSString*)method with:(NSString*)msg;
@end
