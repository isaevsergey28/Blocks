using System;
using System.Collections;
using com.adjust.sdk;
using UnityEngine;
using UnityEngine.Networking;

namespace SayKitInternal
{

    [Serializable]
    public class AttributionRequestData
    {
        public string adid;
        public string network;
        public string adgroup;
        public string campaign;
        public string creative;
        public string clickLabel;
        public string trackerName;
        public string trackerToken;
    }

    [Serializable]
    public class AttributionResponseData
    {
        public string error;

        public int reloadConfig;
        public int success;

        public int retry;
    }


    public class AttributionManager
    {
        public static string Attribution = "";
        public static string AttributionToken = "";

        private static readonly string _url = "https://live.saygames.io/live/attribution";
        private static readonly int _requestTimeout = 5;

        private readonly static string SAYKIT_ATTRIBUTION_FIRST_START = "SAYKIT_ATTRIBUTION_FIRST_START";
        private readonly static string SAYKIT_ATTRIBUTION_REQUEST_COUNT = "SAYKIT_ATTRIBUTION_REQUEST_COUNT";


        public static void RunAttribution()
        {

            SayKitDebug.Log("SayKit: AttributionManager RunAttribution");

            string key = "saykit_attribution_settings";
            string sayKitRemoteSettingsFilePath = Application.persistentDataPath + "/" + key;

            String data = "";


            try
            {

                if (System.IO.File.Exists(sayKitRemoteSettingsFilePath))
                {
                    data = System.IO.File.ReadAllText(sayKitRemoteSettingsFilePath);
                }
                else
                {
                    TextAsset targetFile = Resources.Load<TextAsset>(key);
                    if (targetFile != null)
                    {
                        data = targetFile.text;
                    }
                }

                if (data?.Length > 0 && data[0] == '{')
                {
                    var config = JsonUtility.FromJson<AttributionData>(data);
                    Attribution = config.Attribution;
                    AttributionToken = config.AttributionToken;

                    if (Attribution == "adjust")
                    {
                        AdjustConfig adjustConfig = new AdjustConfig(AttributionToken, AdjustEnvironment.Production);
                        adjustConfig.setLogLevel(AdjustLogLevel.Verbose);
                        adjustConfig.setNeedsCost(true);

                        if (SayKit.config.attributionConfigUpdate && AnalyticsEvent.Level == 0)
                        {
                            if (PlayerPrefs.GetInt(SAYKIT_ATTRIBUTION_FIRST_START) == 0)
                            {
                                PlayerPrefs.SetInt(SAYKIT_ATTRIBUTION_FIRST_START, 1);


                                SayKit.GetInstance().StartCoroutine(WaitingForAdjustAttribution());
                                SayKitDebug.Log("SayKit: Attribution: Delegate set up.");
                            }
                        }

                        Adjust.start(adjustConfig);
                        SayKit.trackEvent("attribution", "adjust");

                        SayKitDebug.Log("SayKit: AttributionManager adjust selected.");
                    }
                    else if (Attribution == "tenjin")
                    {
                        AnalyticsTenjin.init();
                        SayKit.trackEvent("attribution", "tenjin");

                        SayKitDebug.Log("SayKit: AttributionManager tenjin selected.");
                    }
                    else
                    {
                        Debug.LogError("SayKit: Attribution is not selected. Name: " + Attribution);
                        SayKit.trackEvent("attribution_error", "Attribution is not selected. Name: " + Attribution);
                    }
                }

            }
            catch (Exception exc)
            {
                SayKitDebug.Log("SayKit: AttributionManager exception - " + exc.Message);
                SayKit.trackEvent("attribution_error", exc.Message);
            }
        }



        private static int adjustAttributionRetryCount = 0;
        private static bool adjustAttributionChecked = false;

        private static IEnumerator WaitingForAdjustAttribution()
        {
            while (!adjustAttributionChecked)
            {
                SayKitDebug.Log("SayKit: WaitingForAdjustAttribution.");

                if (adjustAttributionRetryCount > 0)
                {
                    SayKitDebug.Log("SayKit: WaitingForAdjustAttribution. Skip because of adjustAttributionRetryCount = " + adjustAttributionRetryCount);
                    
                    yield return new WaitForSecondsRealtime(adjustAttributionRetryCount);

                    adjustAttributionRetryCount = 0;
                }


                AdjustAttribution attribution = Adjust.getAttribution();
                if (attribution != null)
                {
                    yield return adjustAttributionChanged(attribution);


                    Debug.Log("SayKit: WaitingForAdjustAttribution. Done!");
                }
                
                yield return new WaitForSecondsRealtime(1f);
            }

            yield break;
        }


        public static IEnumerator adjustAttributionChanged(AdjustAttribution attribution)
        {
            SayKitDebug.Log("SayKit: adjustAttributionChangedDelegate.");

            if (SayKit.config.attributionConfigUpdate)
            {
                AttributionRequestData data = new AttributionRequestData
                {
                    adid = attribution.adid,
                    adgroup = attribution.adgroup,
                    campaign = attribution.campaign,
                    clickLabel = attribution.clickLabel,

                    creative = attribution.creative,
                    network = attribution.network,
                    trackerName = attribution.trackerName,
                    trackerToken = attribution.trackerToken
                };


                var jsonData = JsonUtility.ToJson(data);

                SayKitDebug.Log("SayKit: Attribution: " + jsonData);

                yield return CheckConfig(jsonData);
            }
        }


        private static String GetUrl()
        {
            var attempt = PlayerPrefs.GetInt(SAYKIT_ATTRIBUTION_REQUEST_COUNT);
            attempt++;


            string appVersion = SayKit.remoteConfig.runtime.segment == 0 ? SayKit.runtimeInfo.version : SayKit.runtimeInfo.version + "." + SayKit.remoteConfig.runtime.segment;

            string url = _url + "?idfa=" + SayKit.runtimeInfo.idfa
                + "&idfv=" + SayKit.runtimeInfo.idfv
                + "&appKey=" + SayKit.config.appKey
                + "&version=" + SayKit.runtimeInfo.version
                + "&ts=" + Utils.currentTimestamp
                + "&installTs=" + ImpressionLogManager.Instance.ImpressionData.FirstStartTimestamp
                + "&saykit=" + SayKit.GetVersion
                + "&config_version=" + appVersion
                + "&attempt=" + attempt;

            
            PlayerPrefs.SetInt(SAYKIT_ATTRIBUTION_REQUEST_COUNT, attempt);

            return url;
        }

        public static IEnumerator CheckConfig(string jsonData) {

            string url = GetUrl();

            SayKitDebug.Log("SayKit: Attribution CheckConfig " + url);


            AttributionResponseData attributionResponse = new AttributionResponseData();

            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                webRequest.SetRequestHeader("Accept", "application/json");

                webRequest.timeout = _requestTimeout;
                
                yield return webRequest.SendWebRequest();


                if (webRequest.isNetworkError)
                {
                    SayKitDebug.Log("SayKit: RequestConversionData: " + webRequest.error);
                }
                else
                {
                    try
                    {
                        SayKitDebug.Log("Attribution: " + webRequest.downloadHandler.text);
                        attributionResponse = JsonUtility.FromJson<AttributionResponseData>(webRequest.downloadHandler.text);
                    }
                    catch (Exception exp)
                    {
                        SayKitDebug.Log("Attribution: error: " + exp.Message);
                    }

                    
                    adjustAttributionRetryCount = attributionResponse.retry;
                    if (adjustAttributionRetryCount <= 0)
                    {
                        adjustAttributionChecked = true;
                    }
                    

                    if (attributionResponse.reloadConfig == 1)
                    {
                        // double check
                        adjustAttributionChecked = true;


                        SayKitDebug.Log("Attribution: Reload Config");


                        yield return RemoteConfigManager.initRoutine();
                        SayKit.initializeRemoteConfig();


                        SayKitDebug.Log("Attribution: update segment " + SayKit.remoteConfig.runtime.segment);

                        SayKit.config.remoteConfigUpdated?.Invoke();
                    }
                    else
                    {
                        if (attributionResponse.success == 0)
                        {
                            SayKitDebug.Log("Attribution: Error: " + attributionResponse.error);
                        }
                    }
                }

            }
        }


        public static void OnApplicationPause(bool pauseStatus)
        {
            SayKitDebug.Log("SayKit: AttributionManager OnPause.");

#if UNITY_IOS
            // No action, iOS SDK is subscribed to iOS lifecycle notifications.
#elif UNITY_ANDROID

            if (Attribution == "adjust")
            {
                if (pauseStatus)
                {
                    AdjustAndroid.OnPause();
                }
                else
                {
                    AdjustAndroid.OnResume();
                }
            }
#endif
        }


        public static void TrackInAppEvent(InAppItem inAppItem)
        {
            if (Attribution == "adjust")
            {
                if (!String.IsNullOrEmpty(inAppItem.adjustToken))
                {
                    AdjustEvent adjustEvent = new AdjustEvent(inAppItem.adjustToken);
                    adjustEvent.setRevenue(inAppItem.price, inAppItem.currency);
                    adjustEvent.setTransactionId(inAppItem.transaction_id);

                    Adjust.trackEvent(adjustEvent);
                }
            }
        }

    }
}
