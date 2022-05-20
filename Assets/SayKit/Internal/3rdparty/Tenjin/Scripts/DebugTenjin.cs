using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DebugTenjin : BaseTenjin {

	public override void Connect(){
		SayKitDebug.Log ("Connecting " + ApiKey);
	}

	public override void Connect(string deferredDeeplink){
		SayKitDebug.Log ("Connecting with deferredDeeplink " + deferredDeeplink);
	}

	public override void Init(string apiKey){
		SayKitDebug.Log ("Initializing " + apiKey);
		ApiKey = apiKey;
	}

	public override void Init(string apiKey, string sharedSecret){
		SayKitDebug.Log ("Initializing with secret " + apiKey);
		ApiKey = apiKey;
		SharedSecret = sharedSecret;
	}

	public override void SendEvent (string eventName){
		SayKitDebug.Log ("Sending Event " + eventName);
	}

	public override void SendEvent (string eventName, string eventValue){
		SayKitDebug.Log ("Sending Event " + eventName + " : " + eventValue);
	}

	public override void Transaction(string productId, string currencyCode, int quantity, double unitPrice, string transactionId, string receipt, string signature){
		SayKitDebug.Log ("Transaction " + productId + ", " + currencyCode + ", " + quantity + ", " + unitPrice + ", " + transactionId + ", " + receipt + ", " + signature);
	}

	public override void GetDeeplink(Tenjin.DeferredDeeplinkDelegate deferredDeeplinkDelegate) {
		SayKitDebug.Log ("Sending DebugTenjin::GetDeeplink");
	}

	public override void OptIn(){
		SayKitDebug.Log ("OptIn ");
	}

	public override void OptOut(){
		SayKitDebug.Log ("OptOut ");
	}

	public override void OptInParams(List<string> parameters){
		SayKitDebug.Log ("OptInParams");
	}

	public override void OptOutParams(List<string> parameters){
		SayKitDebug.Log ("OptOutParams" );
	}
}
