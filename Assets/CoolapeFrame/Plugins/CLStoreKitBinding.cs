using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Coolape
{
	#if UNITY_IPHONE
	public class CLStoreKitBinding
	{
	
		[DllImport ("__Internal")]
		private static extern void _initWithGameObjectName (string listener);

		public static void init (string listener)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				_initWithGameObjectName (listener);
		}

		[DllImport ("__Internal")]
		private static extern bool _CanMakePay ();

		public static bool canMakePayments
		{
			get {
				if (Application.platform == RuntimePlatform.IPhonePlayer)
					return _CanMakePay ();
				return false;
			}
		}

		[DllImport ("__Internal")]
		private static extern void _GetProductList (string productIdentifier);
 
		// Accepts an array of product identifiers. All of the products you have for sale should be requested in one call.
		public static void requestProductData (string[] productIdentifiers)
		{
			foreach (string name in productIdentifiers) {
				Debug.Log ("name = " + name);
			}
			if (Application.platform == RuntimePlatform.IPhonePlayer) {
				_GetProductList (string.Join (",", productIdentifiers));
			}
		}

		/// <summary>
		/// Requests the product data.
		/// </summary>
		/// <param name="productIdentifiersStrs">Product identifiers strs.</param> "productid1,productid2,product3"
		public static void requestProductData (string productIdentifiersStrs)
		{
			Debug.Log ("name = " + productIdentifiersStrs);
			_GetProductList (productIdentifiersStrs);
		}

		[DllImport ("__Internal")]
		private static extern void _PurchaseProduct (string productIdentifier);

		// Purchases the given product and quantity
		public static void purchaseProduct (string productIdentifier)
		{
			Debug.Log ("productIdentifier : " + productIdentifier);
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				_PurchaseProduct (productIdentifier);
		}
	}
	#endif
}
