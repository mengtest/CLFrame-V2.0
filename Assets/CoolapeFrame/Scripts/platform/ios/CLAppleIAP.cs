using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Coolape
{
	/// <summary>
	/// CLIA.苹果内购
	/// </summary>
	public class CLAppleIAP : MonoBehaviour
	{
		public static CLAppleIAP self;

		public CLAppleIAP ()
		{
			self = this;
		}

		public bool isInit = false;
		//		Queue<string> finishedTransactions = new Queue<string> ();
		[HideInInspector]
		public bool isFinishGetProducts = false;
		public object finishBuyCallbck;
		public object finishBuyCallbckOrgsParamater;
		public object finishGetProductsCallbck;
		public object finishGetProductsCallbckOrgsParamater;
		public string productsJson = "";

		#if UNITY_IPHONE
		// Use this for initialization
		void Start ()
		{
			isInit = true;
			CLStoreKitBinding.init (gameObject.name);
		}

		/// <summary>
		/// Buy the specified productIdentifier, callbck and orgs. 购买
		/// </summary>
		/// <param name="productIdentifier">Product identifier.</param> 商品标识
		/// <param name="callbck">Callbck.</param> 回调
		/// 回调参数1：bool＝》true表示购买成功
		/// 回调参数2：string＝》当购买成功时，返回交易json传，失败时返回msg
		/// 回调参数3：obj＝》把之前传过来的透传参数再传回去
		/// <param name="orgs">Orgs.</param> 透传参数
		public void buy (string productIdentifier, object callbck, object orgs)
		{
			finishBuyCallbck = callbck;
			finishBuyCallbckOrgsParamater = orgs;
			if (!CLStoreKitBinding.canMakePayments) {
				Debug.LogWarning ("CLStoreKitBinding.canMakePayments==" + CLStoreKitBinding.canMakePayments);
				return;
			}
			CLStoreKitBinding.purchaseProduct (productIdentifier);
		}

		/// <summary>
		/// Requests the product data.请求商品列表
		/// </summary>
		/// <param name="productIdentifiers">Product identifiers.</param> 商品标识列表
		public void requestProductData (string[] productIdentifiers, object callback, object orgs)
		{
			finishGetProductsCallbck = callback;
			finishGetProductsCallbckOrgsParamater = orgs;
			if (isFinishGetProducts) {
				Utl.doCallback (finishGetProductsCallbck, productsJson, finishGetProductsCallbckOrgsParamater);
			} else {
				CLStoreKitBinding.requestProductData (productIdentifiers);
			}
		}

		/// <summary>
		/// Requests the product data.请求商品列表
		/// </summary>
		/// <param name="productIdentifiersStrs">Product identifiers strs.</param> 商品标识列表串，形式如：“aa,bb,cc,dd”
		public void requestProductData (string productIdentifiersStrs, object callback, object orgs)
		{
			finishGetProductsCallbck = callback;
			finishGetProductsCallbckOrgsParamater = orgs;
			if (isFinishGetProducts) {
				Utl.doCallback (finishGetProductsCallbck, productsJson, finishGetProductsCallbckOrgsParamater);
			} else {
				CLStoreKitBinding.requestProductData (productIdentifiersStrs);
			}
		}

		//		public void LateUpdate ()
		//		{
		//			if (finishedTransactions.Count > 0) {
		//				OnPurchasedSuccess (finishedTransactions.Dequeue ());
		//			}
		//		}
	
		//===================================================
		//=======callback =====================================
		public void OnPurchasedSuccess (string transactionJosn)
		{
			// the transactionJosn:
			//			[[NSString alloc]initWithFormat:@"{\"productIdentifier\":\"%@\",\"transactionIdentifier\":\"%@\",\"transactionReceipt\":\"%@\"}",
			//				transaction.payment.productIdentifier,
			//				transaction.transactionIdentifier,
			//				base64Str];
			Utl.doCallback (finishBuyCallbck, true, transactionJosn, finishBuyCallbckOrgsParamater);
			
//			string[] transactionInfor = transaction.Split (',');
//				string productIdentifier = transactionInfor [0];
//				string transactionID = transactionInfor [1];
//				string base64EncodedTransactionReceipt = transactionInfor [2];
//				//Notify service
//				Hashtable param = new Hashtable ();
//				param.Add ("svcid", Cfg.self.serverID);
//				param.Add ("chn", Cfg.Channel);
//				param.Add ("pcid", GlVar.userInfor.uid);
//				param.Add ("tranid", transactionID);
//				param.Add ("iden", productIdentifier);
//				param.Add ("sign", base64EncodedTransactionReceipt);
//				string path = StrEx.make ("svcid=${svcid}&chn=${chn}&pcid=${pcid}&tranid=${tranid}&iden=${iden}&sign=${sign}", param);
//				Debug.Log ("path = " + path);
//				StartCoroutine (send4Server (path));
//				NPanelAlert.show (AlertSize.SmallAlert, Utl.getLocString ("PaymentSuccess"), null, false);
		}

		//		IEnumerator send4Server (string path)
		//		{
		//			yield return null;
		//			HttpRequest.httpRequest (Cfg.self.gateHost, Cfg.self.gatePort, "payIos", System.Text.Encoding.ASCII.GetBytes (path), 1);
		//		}

		void  OnPurchasedFailed (string msg)
		{
			Utl.doCallback (finishBuyCallbck, false, msg, finishBuyCallbckOrgsParamater);
		}

		void OnPurchaseCancelled (string msg)
		{
			Utl.doCallback (finishBuyCallbck, false, msg, finishBuyCallbckOrgsParamater);
		}

		void OnReceivProductList (string productJson)
		{
			isFinishGetProducts = true;
			productsJson = productJson;
			Utl.doCallback (finishGetProductsCallbck, productsJson, finishGetProductsCallbckOrgsParamater);
		}
		#endif
	}
}