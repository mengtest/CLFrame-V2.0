//
//  IAPHelper.m
//  Unity-iPhone
//
//  Created by chenbin on 14-3-25.
//
//

#import "IAPHelper.h"

static IAPHelper* _iapHelper;
const NSString* OnPurchasedSuccess = @"OnPurchasedSuccess";
const NSString* OnPurchasedFailed = @"OnPurchasedFailed";
const NSString* OnPurchaseCancelled = @"OnPurchaseCancelled";
const NSString* ProductListReceivedEvent = @"OnReceivProductList";
static NSMutableDictionary * productsMap;

@implementation IAPHelper

+(IAPHelper*) instance
{
    if (_iapHelper != nil) {
        return _iapHelper;
    } else {
        _iapHelper =(IAPHelper*)([[IAPHelper alloc] init]);
        
    }
    return _iapHelper;
}
-(void) initWithGameObjectName:(NSString*) goName
{
    gameObjectName = goName;

}
-(id)init
{
    if ((self = [super init])) {
        productsMap = [[NSMutableDictionary alloc] init];
        //---------------------
        //----监听购买结果
        [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    }
    return self;
}

/*
 -(void)buy:
 {
 buyType = type;
 if ([SKPaymentQueue canMakePayments]) {
 //[[SKPaymentQueue defaultQueue] restoreCompletedTransactions];
 [self RequestProductData];
 NSLog(@"允许程序内付费购买");
 }
 else
 {
 NSLog(@"不允许程序内付费购买");
 UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:@"Alert"
 message:@"You can‘t purchase in app store（Himi说你没允许应用程序内购买）"
 delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
 
 [alerView show];
 [alerView release];
 
 }
 }
 */

-(bool)CanMakePay
{
    return [SKPaymentQueue canMakePayments];
}

-(void)RequestProductData :(NSArray*)product
{
    NSLog(@"---------请求对应的产品信息------------");
    NSSet *nsset = [NSSet setWithArray:product];
    SKProductsRequest *request=[[SKProductsRequest alloc] initWithProductIdentifiers: nsset];
    request.delegate = self;
    [request start];
}

//<SKProductsRequestDelegate> 请求协议
//收到的产品信息
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response{
    
    NSLog(@"-----------收到产品反馈信息--------------");
    NSArray *myProduct = response.products;
    NSLog(@"产品Product ID:%@",response.invalidProductIdentifiers);
    NSLog(@"产品付费数量: %lu", [myProduct count]);
    // populate UI
    NSString* json = nil;
    for(SKProduct *product in myProduct){
        [productsMap setObject:product forKey:product.productIdentifier];
        if(json == nil) {
            json = [NSString stringWithFormat:@"{\"productIdentifier\":\"%@\",\"price\":\"%@\",\"localizedTitle\":\"%@\",\"description\":\"%@\",\"localizedDescription\":\"%@\"}",
                    product.productIdentifier,product.price,product.localizedTitle, product.description, product.localizedDescription];
        }else{
            json = [NSString stringWithFormat:@"%@,{\"productIdentifier\":\"%@\",\"price\":\"%@\",\"localizedTitle\":\"%@\",\"description\":\"%@\",\"localizedDescription\":\"%@\"}",
                    json, product.productIdentifier,product.price,product.localizedTitle, product.description, product.localizedDescription];
        }
//        NSLog(@"SKProduct 描述信息%@", [product description]);
//        NSLog(@"产品标题 %@" , product.localizedTitle);
//        NSLog(@"产品描述信息: %@" , product.localizedDescription);
//        NSLog(@"价格: %@" , product.price);
//        NSLog(@"Product id: %@" , product.productIdentifier);
    }
        if(json != nil) {
            json = [NSString stringWithFormat:@"[%@]", json];
        }
    NSLog(@"product info:%@",json);
    [self sendUnityMessage:ProductListReceivedEvent with:json];
    
}

-(void) pay:(NSString*) productIdentifier{
//    SKPayment *payment = [SKPayment paymentWithProductIdentifier:productIdentifier];
    SKPayment *payment = nil;
#if __IPHONE_OS_VERSION_MAX_ALLOWED < __IPHONE_5_0 // 当前支持的sdk版本是否低于5.0
    payment = [SKPayment paymentWithProductIdentifier:productIdentifier];
#else
    SKProduct *p = (SKProduct*)([productsMap objectForKey:productIdentifier]);
    if (p != nil) {
        payment = [SKPayment paymentWithProduct:p];
    } else {
        payment = [SKPayment paymentWithProductIdentifier:productIdentifier];
    }
    
#endif
    
    NSLog(@"---------发送购买请求------------");
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

- (void)requestProUpgradeProductData
{
    NSLog(@"------请求升级数据---------");
    NSSet *productIdentifiers = [NSSet setWithObject:@"com.productid"];
    SKProductsRequest* productsRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    productsRequest.delegate = self;
    [productsRequest start];
    
}
//弹出错误信息
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error{
    NSLog(@"-------弹出错误信息----------");
    
#if __IPHONE_OS_VERSION_MAX_ALLOWED < __IPHONE_9_0 // 当前支持的sdk版本是否低于5.0
    UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Alert",NULL) message:[error localizedDescription]
                                                       delegate:nil cancelButtonTitle:NSLocalizedString(@"Close",nil) otherButtonTitles:nil];
    
    [alerView show];
    #if !__has_feature(objc_arc)
    [alerView release];
    #endif
#else
    UIAlertController* alert = [UIAlertController alertControllerWithTitle:NSLocalizedString(@"Alert",NULL)
                                                                   message:[error localizedDescription]
                                                            preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction* defaultAction = [UIAlertAction actionWithTitle:NSLocalizedString(@"Close",nil)
                                                            style:UIAlertActionStyleDefault
                                                          handler:^(UIAlertAction * action) {}];
    
    [alert addAction:defaultAction];
    [[IAPHelper currentViewController] presentViewController:alert animated:YES completion:nil];
    #endif
}

+ (UIViewController*)currentViewController{
    UIViewController* vc = [UIApplication sharedApplication].keyWindow.rootViewController;
    
    while (1) {
        if ([vc isKindOfClass:[UITabBarController class]]) {
            vc = ((UITabBarController*)vc).selectedViewController;
        }
        
        if ([vc isKindOfClass:[UINavigationController class]]) {
            vc = ((UINavigationController*)vc).visibleViewController;
        }
        
        if (vc.presentedViewController) {
            vc = vc.presentedViewController;
        }else{
            break;
        }
        
    }
    
    return vc;
}

-(void) requestDidFinish:(SKRequest *)request
{
    NSLog(@"----------反馈信息结束--------------");
    
}

-(void) PurchasedTransaction: (SKPaymentTransaction *)transaction{
    NSLog(@"-----PurchasedTransaction----");
    NSArray *transactions =[[NSArray alloc] initWithObjects:transaction, nil];
    [self paymentQueue:[SKPaymentQueue defaultQueue] updatedTransactions:transactions];
    #if !__has_feature(objc_arc)
    [transactions release];
    #endif
}

//<SKPaymentTransactionObserver> 千万不要忘记绑定，代码如下：
//----监听购买结果
//[[SKPaymentQueue defaultQueue] addTransactionObserver:self];

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions//交易结果
{
    NSLog(@"-----paymentQueue--------");
    
    for (SKPaymentTransaction *transaction in transactions)
    {
        switch (transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchased://交易完成
                [self completeTransaction:transaction];
                NSLog(@"-----交易完成 --------");
                /*
                 UIAlertView *alerView =  [[UIAlertView alloc] initWithTitle:@"Alert"
                 message:@"Himi说你购买成功啦～娃哈哈"
                 delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
                 
                 [alerView show];
                 [alerView release];
                 */
                
                break;
            case SKPaymentTransactionStateFailed://交易失败
                [self failedTransaction:transaction];
                NSLog(@"-----交易失败 --------");
                /*
                 UIAlertView *alerView2 =  [[UIAlertView alloc] initWithTitle:@"Alert"
                 message:@"Himi说你购买失败，请重新尝试购买～"
                 delegate:nil cancelButtonTitle:NSLocalizedString(@"Close（关闭）",nil) otherButtonTitles:nil];
                 
                 [alerView2 show];
                 [alerView2 release];
                 */
                break;
            case SKPaymentTransactionStateRestored://已经购买过该商品
                [self restoreTransaction:transaction];
                NSLog(@"-----已经购买过该商品 --------");
            case SKPaymentTransactionStatePurchasing:      //商品添加进列表
                NSLog(@"-----商品添加进列表 --------");
                break;
            default:
                break;
        }
    }
}
- (void) completeTransaction: (SKPaymentTransaction *)transaction
{
    NSLog(@"-----completeTransaction--------");
    NSString* rets = nil;
    NSData *receipt = nil;
    NSString *base64Str = nil;
    if (floor(NSFoundationVersionNumber) < NSFoundationVersionNumber_iOS_7_0) {
        base64Str = [receipt base64Encoding];
    } else {
        base64Str = [receipt base64EncodedStringWithOptions:NSDataBase64Encoding64CharacterLineLength];
    }
            
    if (floor(NSFoundationVersionNumber) < NSFoundationVersionNumber_iOS_7_0) {
        // Load resources for iOS 7.0 or earlier
        receipt = transaction.transactionReceipt;
    } else {
        NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
        receipt = [NSData dataWithContentsOfURL:receiptURL];
    }
    
    rets = [[NSString alloc]initWithFormat:@"{\"productIdentifier\":\"%@\",\"transactionIdentifier\":\"%@\",\"transactionReceipt\":\"%@\"}",
            transaction.payment.productIdentifier,
            transaction.transactionIdentifier,
            base64Str];
    [self sendUnityMessage:OnPurchasedSuccess with:rets];
    
    
    // Remove the transaction from the payment queue.
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
}

//记录交易
-(void)recordTransaction:(NSString *)product{
    NSLog(@"-----记录交易--------");
}

//处理下载内容
-(void)provideContent:(NSString *)product{
    NSLog(@"-----下载--------");
}

- (void) failedTransaction: (SKPaymentTransaction *)transaction{
    NSLog(@"失败");
    if (transaction.error.code == SKErrorPaymentCancelled)
    {
        [self sendUnityMessage:OnPurchaseCancelled with:transaction.error.description];
    } else {
        [self sendUnityMessage:OnPurchasedFailed with:transaction.error.description];
    }
    [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
    
}

-(void) sendUnityMessage:(const NSString*)method with:(NSString*)msg
{
    if(gameObjectName != nil && msg != nil) {
        UnitySendMessage([gameObjectName UTF8String],
                         [method UTF8String], [msg UTF8String]);
    }
}
-(void) paymentQueueRestoreCompletedTransactionsFinished: (SKPaymentTransaction *)transaction{
    
}

- (void) restoreTransaction: (SKPaymentTransaction *)transaction
{
    NSLog(@" 交易恢复处理");
    [self completeTransaction:transaction];
}

-(void) paymentQueue:(SKPaymentQueue *) paymentQueue restoreCompletedTransactionsFailedWithError:(NSError *)error{
    NSLog(@"-------paymentQueue----");
}

#pragma mark connection delegate
- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data
{
#if !__has_feature(objc_arc)
    NSLog(@"%@",  [[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]);
#else
    NSLog(@"%@",  [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]);
#endif
}
- (void)connectionDidFinishLoading:(NSURLConnection *)connection{
    
}

- (void)connection:(NSURLConnection *)connection didReceiveResponse:(NSURLResponse *)response{
    switch([(NSHTTPURLResponse *)response statusCode]) {
        case 200:
        case 206:
            break;
        case 304:
            break;
        case 400:
            break;
        case 404:
            break;
        case 416:
            break;
        case 403:
            break;
        case 401:
        case 500:
            break;
        default:
            break;
    }
}

- (void)connection:(NSURLConnection *)connection didFailWithError:(NSError *)error {
    NSLog(@"test");
}

-(void)dealloc
{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];//解除监听
    
#if !__has_feature(objc_arc)
    [super dealloc];
    [productsMap dealloc];
#endif
}
@end

#if __cplusplus
extern "C" {
#endif
    void _initWithGameObjectName(const char* goName);
    BOOL _CanMakePay();
    void _GetProductList(const char* product);
    void _PurchaseProduct(const char* product);
    
#if __cplusplus
}
#endif


void _initWithGameObjectName(const char* goName) {
#if !__has_feature(objc_arc)
    [[IAPHelper instance] initWithGameObjectName:[[NSString stringWithUTF8String:goName] retain]];
#else
    [[IAPHelper instance] initWithGameObjectName:[NSString stringWithUTF8String:goName]];
#endif
}

BOOL _CanMakePay() {
    return [[IAPHelper instance] CanMakePay];
}

void _PurchaseProduct(const char* product) {
    NSString *productIdentifi = [NSString stringWithUTF8String:product];
    [[IAPHelper instance] pay:productIdentifi];
}

void _GetProductList(const char* products) {
    NSString *productIdentifis = [NSString stringWithUTF8String:products];
    [[IAPHelper instance] RequestProductData: [productIdentifis componentsSeparatedByString:@","]];
}


