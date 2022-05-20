using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SayKitInternal
{

    [Serializable]
    public class InAppData
    {
        public List<InAppItem> Data;
    }

    [Serializable]
    public class InAppItem
    {
        public string product_id;
        public string currency;
        public float price;

        public bool eventSent;
        public bool serverCheck;
        public bool serverError;

        public string adjustToken;
        public string transaction_id;

#if UNITY_IOS
        public string receipt;
#elif UNITY_ANDROID
        public string json;
        public string signature;
#endif

        public bool IsProcessing { get; set; }
    }

    [Serializable]
    public class InAppResponseData
    {
        public bool success;
        public bool seen_before;
        public string message;

        public string adjust_iap_token;
    }

#if UNITY_IOS

    [Serializable]
    public class InAppRerequestData
    {
        public string product_id;
        public string currency;
        public float price;

        public string transaction_id;
        public string receipt;
    }

#elif UNITY_ANDROID

    [Serializable]
    public class InAppRerequestData
    {
        public string product_id;
        public string currency;
        public float price;

        public string json;
        public string signature;

        public string transaction_id;
    }

#endif



    public class InAppManager
    {
        public static InAppManager Instance { get; } = new InAppManager();


        private readonly object _lockSaveObject = new object();
        private readonly object _lockAnalyzeInAppData = new object();

        private readonly string _fileName = "saykit_inapp_temp.txt";
        private readonly int _requestTimeout = 15;

        private readonly string _url = "https://live.saygames.io/iap/verify/";

        private InAppData _inAppData = new InAppData();


        public void Init()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;
            if (!(System.IO.File.Exists(cachePath)))
            {
                System.IO.File.Create(cachePath);

                _inAppData.Data = new List<InAppItem>();

                SaveInAppData();
            }
            else
            {
                ReadInAppData();
            }
        }

        public void CheckInApp(InAppItem inAppItem)
        {
            SayKit.GetInstance().StartCoroutine(Instance.StartInAppChecking(inAppItem));
        }

        private IEnumerator StartInAppChecking(InAppItem item)
        {
            lock (_lockAnalyzeInAppData)
            {
                AddNewInAppItem(item);
            }

            yield return AnalyzeInAppData();
        }

        public IEnumerator AnalyzeInAppData()
        {
            lock (_lockAnalyzeInAppData)
            {
                var items = _inAppData.Data.Where(t => !t.serverCheck && !t.serverError);

                for (int i = 0; i < _inAppData.Data.Count; i++)
                {
                    if (!_inAppData.Data[i].IsProcessing)
                    {
                        _inAppData.Data[i].IsProcessing = true;
                        yield return CheckInAppOnServer(_inAppData.Data[i]);
                    }
                }

                var itemsToRemove = new List<InAppItem>();
                for (int i = 0; i < _inAppData.Data.Count; i++)
                {
                    SendEvent(_inAppData.Data[i]);

                    if (_inAppData.Data[i].eventSent
                        || _inAppData.Data[i].serverError)
                    {
                        itemsToRemove.Add(_inAppData.Data[i]);
                    }
                }

                foreach (var inAppItem in itemsToRemove)
                {
                    _inAppData.Data.Remove(inAppItem);
                }

                SaveInAppData();
            }
        }


        private String GetUrl()
        {
            string url = _url + SayKit.config.appKey
                + "?idfa=" + SayKit.runtimeInfo.idfa
                + "&idfv=" + SayKit.runtimeInfo.idfv
                + "&version=" + SayKit.runtimeInfo.version
                + "&ts=" + Utils.currentTimestamp
                + "&saykit=" + SayKit.GetVersion;

            return url;
        }


        private IEnumerator CheckInAppOnServer(InAppItem inAppItem)
        {
            string url = GetUrl();

            var requestData = new InAppRerequestData();
            requestData.product_id = inAppItem.product_id;
            requestData.price = inAppItem.price;
            requestData.currency = inAppItem.currency;

            requestData.transaction_id = inAppItem.transaction_id;

#if UNITY_IOS
            requestData.receipt = inAppItem.receipt;
#elif UNITY_ANDROID
            requestData.json = inAppItem.json;
            requestData.signature = inAppItem.signature;
#endif

            string postData = JsonUtility.ToJson(requestData);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, postData))
            {
                webRequest.timeout = _requestTimeout;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    SayKitDebug.Log("InAppManager: " + webRequest.error + " * " + inAppItem.product_id);
                }
                else
                {
                    try
                    {
                        SayKitDebug.Log("InAppManager: " + " Answer: " + webRequest.downloadHandler.text);
                        InAppResponseData inAppResponseData = JsonUtility.FromJson<InAppResponseData>(webRequest.downloadHandler.text);

                        if (inAppResponseData.success)
                        {
                            inAppItem.serverCheck = true;
                            inAppItem.eventSent = inAppResponseData.seen_before;
                            inAppItem.adjustToken = inAppResponseData.adjust_iap_token;

                            SendEvent(inAppItem);
                        }
                        else
                        {
                            inAppItem.serverError = true;
                        }

                        SaveInAppData();
                    }
                    catch (Exception exp)
                    {
                        SayKitDebug.Log("InAppManager error: " + exp.Message);
                    }
                }
            }

            inAppItem.IsProcessing = false;
        }


        private void SendEvent(InAppItem inAppItem)
        {
            try
            {
                if (inAppItem.serverCheck)
                {
                    if (!inAppItem.eventSent)
                    {
                        AttributionManager.TrackInAppEvent(inAppItem);

                        Dictionary<string, object> carrencyParam = new Dictionary<string, object>
                            {
                                { "currency", inAppItem.currency}
                            };

                        AnalyticsEvent.trackFullFirebaseEvent("inapp_purchase", inAppItem.price, carrencyParam);

#if UNITY_ANDROID     
                        AnalyticsEvent.trackFacebookPurchaseEvent(inAppItem.price, inAppItem.currency);
#endif

                        inAppItem.eventSent = true;

                        var extra = "{ \"price\":" + inAppItem.price + ",\"currency\":\"" + inAppItem.currency + "\"}";
                        AnalyticsEvent.trackEvent("iap_sdk_event", _inAppData.Data?.Count ?? 0, 0, extra);
                    }
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }
        }


        private void AddNewInAppItem(InAppItem item)
        {
            _inAppData.Data.Add(item);
            SaveInAppData();
        }

        private void SaveInAppData()
        {
            try
            {
                lock (_lockSaveObject)
                {
                    string cachePath = Application.persistentDataPath + "/" + _fileName;
                    var jsonData = JsonUtility.ToJson(_inAppData);

                    System.IO.File.WriteAllText(cachePath, jsonData);
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }
        }

        public void ReadInAppData()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;

            try
            {
                lock (_lockSaveObject)
                {
                    var jsonData = System.IO.File.ReadAllText(cachePath);

                    SayKitDebug.Log("InAppManager: " + jsonData);

                    _inAppData = JsonUtility.FromJson<InAppData>(jsonData);

                    if (_inAppData == null)
                    {
                        _inAppData = new InAppData();
                    }

                    if (_inAppData.Data == null)
                    {
                        _inAppData.Data = new List<InAppItem>();
                    }
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log("InAppManager read error: " + exp.Message);
            }
        }
    }

}