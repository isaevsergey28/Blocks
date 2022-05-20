using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

public class IosTenjin : BaseTenjin {

#if UNITY_IPHONE && !UNITY_EDITOR

	[DllImport ("__Internal")]
	private static extern void iosTenjinInit(string apiKey);

	[DllImport ("__Internal")]
	private static extern void iosTenjinInitWithSharedSecret(string apiKey, string sharedSecret);

	[DllImport ("__Internal")]
	private static extern void iosTenjinConnect();

	[DllImport ("__Internal")]
	private static extern void iosTenjinConnectWithDeferredDeeplink(string deferredDeeplink);

	[DllImport ("__Internal")]
	private static extern void iosTenjinOptIn();

	[DllImport ("__Internal")]
	private static extern void iosTenjinOptOut();

	[DllImport ("__Internal")]
	private static extern void iosTenjinOptInParams(String[] parameters, int size);

	[DllImport ("__Internal")]
	private static extern void iosTenjinOptOutParams(String[] parameters, int size);

	[DllImport ("__Internal")]
	private static extern void iosTenjinSendEvent(string eventName);

	[DllImport ("__Internal")]
	private static extern void iosTenjinSendEventWithValue(string eventName, string eventValue);

	[DllImport ("__Internal")]
	private static extern void iosTenjinTransaction(string productId, string currencyCode, int quantity, double unitPrice);

	[DllImport ("__Internal")]
	private static extern void iosTenjinTransactionWithReceiptData(string productId, string currencyCode, int quantity, double unitPrice, string transactionId, string receipt);

	[DllImport ("__Internal")]
 	private static extern void iosTenjinRegisterDeepLinkHandler(DeepLinkHandlerNativeDelegate deepLinkHandlerNativeDelegate);

	private delegate void DeepLinkHandlerNativeDelegate(IntPtr deepLinkDataPairArray, int deepLinkDataPairCount);
	
	private static readonly Stack<Dictionary<string, string>> deferredDeeplinkEvents = new Stack<Dictionary<string, string>>();
	private static Tenjin.DeferredDeeplinkDelegate registeredDeferredDeeplinkDelegate;

	public override void Init(string apiKey){
		SayKitDebug.Log ("iOS Initializing " + apiKey);
		ApiKey = apiKey;
		iosTenjinInit (ApiKey);
	}

	public override void Init(string apiKey, string sharedSecret){
		SayKitDebug.Log ("iOS Initializing with Shared Secret " + apiKey);
		ApiKey = apiKey;
		SharedSecret = sharedSecret;
		iosTenjinInitWithSharedSecret (ApiKey, SharedSecret);
	}

	public override void Connect(){
		SayKitDebug.Log ("iOS Connecting " + ApiKey);
		iosTenjinConnect();
	}
	
	public override void Connect(string deferredDeeplink){
		SayKitDebug.Log ("iOS Connecting with deferredDeeplink " + deferredDeeplink);
		iosTenjinConnectWithDeferredDeeplink (deferredDeeplink);
	}

	public override void OptIn(){
		SayKitDebug.Log ("iOS OptIn");
		iosTenjinOptIn ();
	}

	public override void OptOut(){
		SayKitDebug.Log ("iOS OptOut");
		iosTenjinOptOut ();
	}

	public override void OptInParams(List<string> parameters){
		SayKitDebug.Log ("iOS OptInParams" + parameters.ToString());
		iosTenjinOptInParams (parameters.ToArray(), parameters.Count);
	}

	public override void OptOutParams(List<string> parameters){
		SayKitDebug.Log ("iOS OptOutParams" + parameters.ToString());
		iosTenjinOptOutParams (parameters.ToArray(), parameters.Count);
	}

	public override void SendEvent(string eventName){
		SayKitDebug.Log ("iOS Sending Event " + eventName);
		iosTenjinSendEvent(eventName);
	}

	public override void SendEvent(string eventName, string eventValue){
		SayKitDebug.Log ("iOS Sending Event " + eventName + " : " + eventValue);
		iosTenjinSendEventWithValue(eventName, eventValue);
	}

	public override void Transaction(string productId, string currencyCode, int quantity, double unitPrice, string transactionId, string receipt, string signature){
		signature = null;

		//only if the receipt and transaction_id are not null, then try to validate the transaction. Otherwise manually record the transaction
		if(receipt != null && transactionId != null){
			SayKitDebug.Log ("iOS Transaction with receipt " + productId + ", " + currencyCode + ", " + quantity + ", " + unitPrice + ", " + transactionId + ", " + receipt);
			iosTenjinTransactionWithReceiptData(productId, currencyCode, quantity, unitPrice, transactionId, receipt);
		}
		else{
			SayKitDebug.Log ("iOS Transaction " + productId + ", " + currencyCode + ", " + quantity + ", " + unitPrice);
			iosTenjinTransaction(productId, currencyCode, quantity, unitPrice);
		}
	}

	public override void GetDeeplink(Tenjin.DeferredDeeplinkDelegate deferredDeeplinkDelegate) {
	SayKitDebug.Log ("Sending IosTenjin::GetDeeplink");
		registeredDeferredDeeplinkDelegate = deferredDeeplinkDelegate;
		iosTenjinRegisterDeepLinkHandler(DeepLinkHandler);
	}

	private void Update() {
		lock (deferredDeeplinkEvents) {
			while (deferredDeeplinkEvents.Count > 0) {
				Dictionary<string, string> deepLinkData = deferredDeeplinkEvents.Pop();
				if (registeredDeferredDeeplinkDelegate != null) {
					registeredDeferredDeeplinkDelegate(deepLinkData);
				}
			}
		}
	}

	[MonoPInvokeCallback(typeof(DeepLinkHandlerNativeDelegate))]
	private static void DeepLinkHandler(IntPtr deepLinkDataPairArray, int deepLinkDataPairCount) {
		if (deepLinkDataPairArray == IntPtr.Zero)
			return;

		Dictionary<string, string> deepLinkData = 
			NativeUtility.MarshalStringStringDictionary(deepLinkDataPairArray, deepLinkDataPairCount);

		lock (deferredDeeplinkEvents) {
			deferredDeeplinkEvents.Push(deepLinkData);
		}
	}

	private static class NativeUtility {
		/// <summary>
		/// Marshals a native linear array of structs to the managed array.
		/// </summary>
		public static T[] MarshalNativeStructArray<T>(IntPtr nativeArrayPtr, int nativeArraySize) where T : struct {
			if (nativeArrayPtr == IntPtr.Zero)
				throw new ArgumentNullException("nativeArrayPtr");

			if (nativeArraySize < 0)
				throw new ArgumentOutOfRangeException("nativeArraySize");

			T[] managedArray = new T[nativeArraySize];
			IntPtr currentNativeArrayPtr = nativeArrayPtr;
			int structSize = Marshal.SizeOf(typeof(T));
			for (int i = 0; i < nativeArraySize; i++) {
				T marshaledStruct = (T) Marshal.PtrToStructure(currentNativeArrayPtr, typeof(T));
				managedArray[i] = marshaledStruct;
				currentNativeArrayPtr = (IntPtr) (currentNativeArrayPtr.ToInt64() + structSize);
			}

			return managedArray;
		}
		
		/// <summary>
		/// Marshals the native representation to a IDictionary&lt;string, string&gt;.
		/// </summary>
		public static Dictionary<string, string> MarshalStringStringDictionary(IntPtr nativePairArrayPtr, int nativePairArraySize) {
			if (nativePairArrayPtr == IntPtr.Zero)
				throw new ArgumentNullException("nativePairArrayPtr");

			if (nativePairArraySize < 0)
				throw new ArgumentOutOfRangeException("nativePairArraySize");

			Dictionary<string, string> dictionary = new Dictionary<string, string>(nativePairArraySize);
			StringStringKeyValuePair[] pairs = MarshalNativeStructArray<StringStringKeyValuePair>(nativePairArrayPtr, nativePairArraySize);
			foreach (StringStringKeyValuePair pair in pairs) {
				dictionary.Add(pair.Key, pair.Value);
			}
			return dictionary;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct StringStringKeyValuePair {
			public string Key;
			public string Value;
		}
	}

#else
	public override void Init(string apiKey){
		SayKitDebug.Log ("iOS Initializing " + apiKey);
		ApiKey = apiKey;
	}

	public override void Init(string apiKey, string sharedSecret){
		SayKitDebug.Log ("iOS Initializing with Shared Secret" + apiKey);
		ApiKey = apiKey;
		SharedSecret = sharedSecret;
	}

	public override void Connect(){
		SayKitDebug.Log ("iOS Connecting " + ApiKey);
	}

	public override void Connect(string deferredDeeplink){
		SayKitDebug.Log ("Connecting with deferredDeeplink " + deferredDeeplink);
	}

	public override void SendEvent(string eventName){
		SayKitDebug.Log ("iOS Sending Event " + eventName);
	}

	public override void SendEvent(string eventName, string eventValue){
		SayKitDebug.Log ("iOS Sending Event " + eventName + " : " + eventValue);
	}

	public override void Transaction(string productId, string currencyCode, int quantity, double unitPrice, string transactionId, string receipt, string signature){
		SayKitDebug.Log ("iOS Transaction " + productId + ", " + currencyCode + ", " + quantity + ", " + unitPrice + ", " + transactionId + ", " + receipt + ", " + signature);
	}

	public override void GetDeeplink(Tenjin.DeferredDeeplinkDelegate deferredDeeplinkDelegate) {
		SayKitDebug.Log ("Sending IosTenjin::GetDeeplink");
	}

	public override void OptIn(){
		SayKitDebug.Log ("iOS OptIn");
	}

	public override void OptOut(){
		SayKitDebug.Log ("iOS OptOut");
	}

	public override void OptInParams(List<string> parameters){
		SayKitDebug.Log ("iOS OptInParams");
	}

	public override void OptOutParams(List<string> parameters){
		SayKitDebug.Log ("iOS OptOutParams");
	}

#endif
}
