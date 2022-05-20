using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using SayGames.Med.Internal;

#if SAYKIT_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace SayKitInternal
{

    class AnalyticsEvent
    {

#if UNITY_EDITOR
        private static void sayKitEventInit(string appKey, string version, string appVersion, int track_waterfall) {}
        private static void sayKitEventTrackFull(string appKey, string idfa, string device_id, string device_os, string device_name, string version, int segment, string eventName, int param1, int param2, string extra, int param3, int param4, string extra2, string tag, int level, int scurrency, int hcurrency) {}
        private static void sayKitEventTrackFirebase(string eventName, string extra) { }
        private static void sayKitEventTrackFirebaseWithValue(string eventName, float extra) { }
        private static void sayKitEventTrackFullFirebase(string logEvent, float valueToSum, string customJSPN) { }

        private static void sayKitSetCrashlyticsParam(string paramName, string paramValue) { }


        private static void sayKitEventTrackFacebook(string eventName, string extra) { }
        private static void sayKitEventTrackFacebookPurchaseEvent(float valueToSum, string currencyCode) { }
        private static void sayKitEventTrackFullFacebook(string logEvent, float valueToSum, string customJSPN) { }

        private static void sayKitEventTrackAvailableMemory() { }
        private static void sayKitEventTrackTotalMemory() { }

        private static int saykitGetApplicationStartTimestamp() { return -1; }

        private static void sayKitTrackSDKVersions() { }
        private static void sayKitTrackFirebaseId() { }

#elif UNITY_IOS

        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFull(string appKey, string idfa, string device_id, string device_os, string device_name, string version, int segment, string eventName, int param1, int param2, string extra,  int param3, int param4, string extra2, string tag, int level, int scurrency, int hcurrency);

        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFirebase(string eventName, string extra);
        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFirebaseWithValue(string eventName, float extra);
        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFullFirebase(string logEvent, float valueToSum, string customJSPN);

        [DllImport("__Internal")]
        private static extern void sayKitSetCrashlyticsParam(string paramName, string paramValue);

        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFacebook(string eventName, string extra);
        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFacebookPurchaseEvent(float valueToSum, string currencyCode);
        [DllImport("__Internal")]
        private static extern void sayKitEventTrackFullFacebook(string logEvent, float valueToSum, string customJSPN);

        [DllImport("__Internal")]
        private static extern void sayKitEventTrackAvailableMemory();

        [DllImport("__Internal")]
        private static extern void sayKitEventTrackTotalMemory();

        [DllImport("__Internal")]
        private static extern int saykitGetApplicationStartTimestamp();

        [DllImport("__Internal")]
        private static extern void sayKitTrackSDKVersions();

        [DllImport("__Internal")]
        private static extern void sayKitTrackFirebaseId();

#elif UNITY_ANDROID
        
        private static readonly AndroidJavaClass SayKitEventsJava = new AndroidJavaClass("by.saygames.SayKitEvents");
        private static void sayKitEventInit(string appKey, string version, string appVersion, int track_waterfall) {
            SayKitEventsJava.CallStatic("init", appKey, version, appVersion, track_waterfall);
        }
        private static void sayKitEventTrackFull(string appKey, string idfa, string device_id, string device_os, string device_name, string version, int segment, string eventName, int param1, int param2, string extra, int param3, int param4, string extra2, string tag, int level, int scurrency, int hcurrency) {
            SayKitEventsJava.CallStatic("trackFull", appKey, idfa, device_id, device_os, device_name, version, segment, eventName, param1, param2, extra, param3, param4, extra2, tag, level, scurrency, hcurrency);
        }


        private static void sayKitEventTrackFirebase(string eventName, string extra) {
            SayKitEventsJava.CallStatic("trackFirebaseEvent", eventName, extra);
        }

        private static void sayKitEventTrackFirebaseWithValue(string eventName, float extra) {
            SayKitEventsJava.CallStatic("trackFirebaseEventWithValue", eventName, extra);
        }

        private static void sayKitEventTrackFullFirebase(string logEvent, float valueToSum, string customJSPN) {
            SayKitEventsJava.CallStatic("trackFullFirebaseEvent", logEvent, valueToSum, customJSPN);
        }


        private static void sayKitSetCrashlyticsParam(string paramName,  string paramValue) {
            SayKitEventsJava.CallStatic("setCrashlyticsParam", paramName, paramValue);
        }


        private static void sayKitEventTrackFacebook(string eventName, string extra) {
            SayKitEventsJava.CallStatic("trackFacebookEvent", eventName, extra);
        }

        private static void sayKitEventTrackFacebookPurchaseEvent(float valueToSum, string currencyCode) {
            SayKitEventsJava.CallStatic("trackFacebookPurchaseEvent", valueToSum, currencyCode);
        }

        private static void sayKitEventTrackFullFacebook(string logEvent, float valueToSum, string customJSPN) {
            SayKitEventsJava.CallStatic("trackFullFacebookEvent", logEvent, valueToSum, customJSPN);
        }


        private static void sayKitEventTrackAvailableMemory() {
            SayKitEventsJava.CallStatic("trackAvailableMemory");
        }

        private static void sayKitEventTrackTotalMemory() { }

        private static int saykitGetApplicationStartTimestamp()
        {
             var timestamp = SayKitEventsJava.CallStatic<int>("getApplicationStartTimestamp");
             return timestamp;
        }

        private static void sayKitTrackSDKVersions() {
            SayKitEventsJava.CallStatic("trackSDKVersions");
        }

        private static void sayKitTrackFirebaseId() {
            SayKitEventsJava.CallStatic("trackFirebaseId");
        }

#endif
        private readonly static string SAYKIT_ANALYTIC_LEVEL = "SAYKIT_ANALYTIC_LEVEL";
        private readonly static string SAYKIT_ANALYTIC_HCURRENCY = "SAYKIT_ANALYTIC_HCURRENCY";
        private readonly static string SAYKIT_ANALYTIC_SCURRENCY = "SAYKIT_ANALYTIC_SCURRENCY";

        private readonly static string SAYKIT_TUTORIAL_HISTORY = "SAYKIT_TUTORIAL_HISTORY";
        private readonly static string SAYKIT_TUTORIAL_STEP = "SAYKIT_TUTORIAL_STEP";

        private readonly static string SAYKIT_ANALYTIC_START_COUNT = "SAYKIT_ANALYTIC_START_COUNT";


        static int _level = 0;
        public static int Level
        {
            get
            {
                if (_level <= 0)
                {
                    _level = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_LEVEL);
                }

                return _level;
            }
            set
            {
                if (value != _level)
                {
                    PlayerPrefs.SetInt(SAYKIT_ANALYTIC_LEVEL, value);
                }

                _level = value;
            }
        }

        static int _hcurrency = 0;
        static int HCurrency
        {
            get
            {
                if (_hcurrency <= 0)
                {
                    _hcurrency = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_HCURRENCY);
                }

                return _hcurrency;
            }
            set
            {
                if (value != _hcurrency)
                {
                    PlayerPrefs.SetInt(SAYKIT_ANALYTIC_HCURRENCY, value);
                }

                _hcurrency = value;
            }
        }
        static int _scurrency = 0;
        static int SCurrency
        {
            get
            {
                if (_scurrency <= 0)
                {
                    _scurrency = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_SCURRENCY);
                }

                return _scurrency;
            }
            set
            {
                if (value != _scurrency)
                {
                    PlayerPrefs.SetInt(SAYKIT_ANALYTIC_SCURRENCY, value);
                }

                _scurrency = value;
            }
        }


        static int levelTimestamp = 0;
        static int levelExtraTimestamp = 0;
        static int levelStageTimestamp = 0;

        static int chunkTimestamp = 0;
        static int chunkSequenceNumber = 0;
        static string chunkName = "";

        static int levelTimestampId = 0 ;
        static string levelExtraTimestampId = "";
        static int levelStageTimestampId = 0;

        private static int _startCount = 0;


        static public void init(string appKey, string version, string appVersion, int track_waterfall)
        {
            tutorialHistory = PlayerPrefs.GetString(SAYKIT_TUTORIAL_HISTORY);
            tutorialStep = PlayerPrefs.GetInt(SAYKIT_TUTORIAL_STEP);


            _startCount = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_START_COUNT);
            _startCount += 1; // increment start count;

            PlayerPrefs.SetInt(SAYKIT_ANALYTIC_START_COUNT, _startCount);


#if UNITY_ANDROID
            sayKitEventInit(appKey, version, appVersion, track_waterfall);
#endif
        }

        static public void trackLevelStarted(string tag, int level)
        {
            trackLevelStarted(tag, level, 0, "", 0, "");
        }

        static public void trackLevelStarted(string tag, int level, int score, string extra)
        {
            trackLevelStarted(tag, level, score, extra, 0, "");
        }

        static public void trackLevelStarted(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
        {
            AnalyticsEvent.Level = level;
            AnalyticsEvent.levelTimestampId = level;
            AnalyticsEvent.levelTimestamp = Utils.currentTimestamp;

            trackTagEvent("level_started", tag, 0, score, eventParam1, extra, eventParam2);
        }


        static public void trackLevelCompleted(string tag, int level, int score)
        {
            trackLevelCompleted(tag, level, score, "", 0, "");
        }

        static public void trackLevelCompleted(string tag, int level, int score, string extra)
        {
            trackLevelCompleted(tag, level, score, extra, 0, "");
        }

        static public void trackLevelCompleted(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
        {
            AnalyticsEvent.Level = level;

            int duration = Utils.currentTimestamp - AnalyticsEvent.levelTimestamp;
            if (AnalyticsEvent.levelTimestampId != level)
            {
                duration = 0;
            }

            trackTagEvent("level_completed", tag, duration, score, eventParam1, extra, eventParam2);
            trackChunkCompleted();

#if UNITY_IOS
            RuntimeInfoManager.OnLevelCompleted(level);
#endif
        }

        static public void trackLevelFailed(string tag, int level, int score)
        {
            trackLevelFailed(tag, level, score, "", 0, "");
        }

        static public void trackLevelFailed(string tag, int level, int score, string extra)
        {
            trackLevelFailed(tag, level, score, extra, 0, "");
        }

        static public void trackLevelFailed(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
        {
            AnalyticsEvent.Level = level;

            int duration = Utils.currentTimestamp - AnalyticsEvent.levelTimestamp;
            if (AnalyticsEvent.levelTimestampId != level)
            {
                duration = 0;
            }


            trackTagEvent("level_failed", tag, duration, score, eventParam1, extra, eventParam2);
            trackChunkFailed();
        }


        static public void trackLevelExtraStarted(string tag)
        {
            AnalyticsEvent.levelExtraTimestampId = tag;
            AnalyticsEvent.levelExtraTimestamp = Utils.currentTimestamp;

            trackTagEvent("level_extra_started", tag, 0, 0);
        }

        static public void trackLevelExtraCompleted(string tag, int score)
        {

            int duration = Utils.currentTimestamp - AnalyticsEvent.levelExtraTimestamp;
            if (AnalyticsEvent.levelExtraTimestampId.Equals(tag))
            {
                duration = 0;
            }

            trackTagEvent("level_extra_completed", tag, duration, score);
            trackChunkCompleted();
        }

        static public void trackLevelExtraFailed(string tag, int score)
        {

            int duration = Utils.currentTimestamp - AnalyticsEvent.levelExtraTimestamp;
            if (AnalyticsEvent.levelExtraTimestampId.Equals(tag))
            {
                duration = 0;
            }

            trackTagEvent("level_extra_failed", tag, duration, score);
            trackChunkFailed();
        }




        static public void trackStageStarted(string tag, int number)
        {
            AnalyticsEvent.levelStageTimestampId = number;
            AnalyticsEvent.levelStageTimestamp = Utils.currentTimestamp;

            trackTagEvent("level_stage_started", tag, 0, 0, number);
        }

        static public void trackStageCompleted(string tag, int number, int score)
        {
            int duration = Utils.currentTimestamp - AnalyticsEvent.levelStageTimestamp;
            if (AnalyticsEvent.levelStageTimestampId != number)
            {
                duration = 0;
            }

            trackTagEvent("level_stage_completed", tag, duration, score, number);
            trackChunkCompleted();
        }

        static public void trackStageFailed(string tag, int number, int score)
        {
            int duration = Utils.currentTimestamp - AnalyticsEvent.levelStageTimestamp;
            if (AnalyticsEvent.levelStageTimestampId != number)
            {
                duration = 0;
            }

            trackTagEvent("level_stage_failed", tag, duration, score, number);
            trackChunkFailed();
        }



        static public void trackChunkStarted(string name, int sequenceNumber)
        {
            trackChunkStarted(name, sequenceNumber, null);
        }

        static public void trackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData)
        {
            AnalyticsEvent.chunkTimestamp = Utils.currentTimestamp;
            chunkSequenceNumber = sequenceNumber;
            chunkName = name;

            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConverterExtensions.toJson(customData);
            }

            trackEvent("chunk_started", chunkSequenceNumber,0, chunkName, 0,0,extra2);
        }


        static public void trackChunkCompleted()
        {
            trackChunkCompleted(null);
        }

        static public void trackChunkCompleted(Dictionary<string, object> customData)
        {
            if (chunkSequenceNumber > 0 && chunkTimestamp > 0)
            {
                int duration = Utils.currentTimestamp - AnalyticsEvent.chunkTimestamp;

                var extra2 = "";
                if (customData != null)
                {
                    extra2 = JsonConverterExtensions.toJson(customData);
                }

                trackEvent("chunk_completed", chunkSequenceNumber, duration, chunkName, 0, 0, extra2);

                chunkSequenceNumber = -1;
                AnalyticsEvent.chunkTimestamp = -1;
            }
        }


        static public void trackChunkFailed()
        {
            trackChunkFailed(null);
        }

        static public void trackChunkFailed(Dictionary<string, object> customData)
        {
            if (chunkSequenceNumber > 0 && chunkTimestamp > 0)
            {
                int duration = Utils.currentTimestamp - AnalyticsEvent.chunkTimestamp;

                var extra2 = "";
                if (customData != null)
                {
                    extra2 = JsonConverterExtensions.toJson(customData);
                }

                trackEvent("chunk_failed", chunkSequenceNumber, duration, chunkName, 0, 0, extra2);

                chunkSequenceNumber = -1;
                AnalyticsEvent.chunkTimestamp = -1;
            }
        }


        static public string tutorialHistory = "";
        static public int tutorialStep = 0;

        static public void trackTutorialStep(string tutorialName, string stepName)
        {
            var newItem = tutorialName + "_" + stepName;
            if (!tutorialHistory.Contains(newItem))
            {

                tutorialStep = tutorialStep + 1;

                trackEvent("tutorial_step", tutorialStep, 0, tutorialName, 0, 0, stepName);
                 
                tutorialHistory += newItem + ", "; 


                PlayerPrefs.SetString(SAYKIT_TUTORIAL_HISTORY, tutorialHistory);
                PlayerPrefs.SetInt(SAYKIT_TUTORIAL_STEP, tutorialStep);
            }
        }



        static public void trackSoftIncome(string eventName, int amount, int total, string place = "")
        {
            AnalyticsEvent.SCurrency = total + amount;
            trackEvent(eventName, amount, total, place);
        }

        static public void trackSoftOutcome(string eventName, int amount, int total, string place = "")
        {
            AnalyticsEvent.SCurrency = total - amount;
            trackEvent(eventName, amount, total, place);
        }

        static public void trackHardIncome(string eventName, int amount, int total, string place = "")
        {
            AnalyticsEvent.HCurrency = total + amount;
            trackEvent(eventName, amount, total, place);
        }

        static public void trackHardOutcome(string eventName, int amount, int total, string place = "")
        {
            AnalyticsEvent.HCurrency = total - amount;
            trackEvent(eventName, amount, total, place);
        }





        static public void trackItem(string item)
        {
            trackEvent("item", item);
        }


        static public void trackItem(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConverterExtensions.toJson(customData);
            }

            trackEvent("item", ownedItems, (int)sourceId, item, 0, 0, extra2);
        }

        static public void trackItemLoss(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
        {
            var extra2 = "";
            if (customData != null)
            {
                extra2 = JsonConverterExtensions.toJson(customData);
            }

            trackEvent("item_loss", ownedItems, (int)sourceId, item, 0, 0, extra2);
        }



        static public void trackEvent(string eventName)
        {
            trackEvent(eventName, 0, 0, "");
        }

        static public void trackEvent(string eventName, int eventParamInt)
        {
            trackEvent(eventName, eventParamInt, 0, "");
        }

        static public void trackEvent(string eventName, int eventParamInt1, int eventParamInt2)
        {
            trackEvent(eventName, eventParamInt1, eventParamInt2, "");
        }

        static public void trackTagEvent(string eventName, string tag, int eventParamInt1, int eventParamInt2)
        {
            trackEvent(eventName, eventParamInt1, eventParamInt2, "", 0, 0, "", tag);
        }

        static public void trackTagEvent(string eventName, string tag, int eventParamInt1, int eventParamInt2, int eventParamInt3)
        {
            trackEvent(eventName, eventParamInt1, eventParamInt2, "", eventParamInt3, 0, "", tag);
        }

        static public void trackTagEvent(string eventName, string tag, int eventParamInt1, int eventParamInt2, string eventParamString)
        {
            trackEvent(eventName, eventParamInt1, eventParamInt2, eventParamString, 0, 0, "", tag);
        }

        static public void trackTagEvent(string eventName, string tag, int eventParamInt1, int eventParamInt2, int eventParamInt3, string eventParamString, string eventParamString2)
        {
            trackEvent(eventName, eventParamInt1, eventParamInt2, eventParamString, eventParamInt3, 0, eventParamString2, tag);
        }


        static public void trackEvent(string eventName, string eventParamString)
        {
            trackEvent(eventName, 0, 0, eventParamString);
        }
        
        static public void trackEvent(string eventName, string eventParamString, string eventParamString2)
        {
            trackEvent(eventName, 0, 0, eventParamString, 0, 0, eventParamString2);
        }


        static public void trackEvent(string eventName, int eventParamInt1, int eventParamInt2, string eventParamString, int eventParamInt3 = 0, int eventParamInt4 = 0, string eventParamString2 = "", string tag = "")
        {

            if (!RuntimeInfoManager.initialized)
            {
                return;
            }


            var segment = SayKit.remoteConfig.runtime.segment;
            if (SayKit.config.overrideAnalyticSegment >= 0)
            {
                segment = SayKit.config.overrideAnalyticSegment;
            }

            sayKitEventTrackFull(
                SayKit.config.appKey,
                SayKit.runtimeInfo.idfa,
                SayKit.runtimeInfo.idfv,
                SayKit.runtimeInfo.deviceOs,
                SayKit.runtimeInfo.deviceModel,
                SayKit.runtimeInfo.version,
                segment,
                eventName,
                eventParamInt1,
                eventParamInt2,
                eventParamString,
                eventParamInt3,
                eventParamInt4,
                eventParamString2,
                tag,
                Level,
                SCurrency,
                HCurrency
            );
        }



        private static IDictionary<string, int> _trackRewardedDictionary = new Dictionary<string, int>();
        private static IDictionary<string, int> _trackInterstitialDictionary = new Dictionary<string, int>();

        private static int _trackOfferDelay = 5;


        public static void trackRewardedOffer(string place)
        {
            checkOffer(place, _trackRewardedDictionary, true);
        }

        public static void trackInterstitialOffer(string place)
        {
            checkOffer(place, _trackInterstitialDictionary, false);
        }

        private static void checkOffer(string place, IDictionary<string, int> dictionary, bool isRewarded)
        {
            if (dictionary.TryGetValue(place, out int lastTimestamp))
            {
                if (SayKitInternal.Utils.currentTimestamp - lastTimestamp >= _trackOfferDelay)
                {
                    dictionary[place] = SayKitInternal.Utils.currentTimestamp;
                    trackOffer(place, isRewarded);
                }
            }
            else
            {
                dictionary.Add(place, SayKitInternal.Utils.currentTimestamp);
                trackOffer(place, isRewarded);
            }
        }

        private static void trackOffer(string place, bool isRewarded)
        {
            if (isRewarded)
            {
                int isAvailable = SayKit.isRewardedAvailable(place) ? 1 : 0;
                int mediationHasRewarded = AdsManager.hasRewarded() ? 1 : 0;
                trackEvent("rewarded_offer", isAvailable, mediationHasRewarded, place);
            }
            else
            {
                int isAvailable = SayKit.isInterstitialAvailable(place) ? 1 : 0;
                int mediationHasInterstitial = AdsManager.hasInterstitial() ? 1 : 0;
                trackEvent("interstitial_offer", isAvailable, mediationHasInterstitial, place);
            }
        }



        static public void trackApplicationLoadDuration()
        {
            var duration = Utils.currentTimestamp - saykitGetApplicationStartTimestamp();
            var startCount = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_START_COUNT);
            
            trackEvent("app_loaded", duration, startCount);
        }

        static public void trackSayKitLoadDuration()
        {
            var duration = Utils.currentTimestamp - saykitGetApplicationStartTimestamp();
            var startCount = PlayerPrefs.GetInt(SAYKIT_ANALYTIC_START_COUNT);

            Debug.Log("SayTest: " + saykitGetApplicationStartTimestamp() + " ||| now: " + Utils.currentTimestamp + " ||| duration: " + duration);

            trackEvent("saykit_loaded", duration, startCount);
        }



        static public void trackFirebaseEvent(string eventName, string eventParamString)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitEventTrackFirebase(eventName, eventParamString);
        }

        static public void trackFirebaseEventWithValue(string eventName, float eventParam)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitEventTrackFirebaseWithValue(eventName, eventParam);
        }

        static public void trackFullFirebaseEvent(string logEvent, float? valueToSum = default(float?), Dictionary<string, object> parameters = null)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            try
            {
                var strongValue = (float)(valueToSum ?? 0);
                var dictJspn = ConvertDictionaryToJSPN(parameters);
                
                sayKitEventTrackFullFirebase(logEvent, strongValue, dictJspn);
            }
            catch (Exception ex)
            {
                SayKitDebug.Log("trackFullFirebaseEvent:" + ex.Message);
            }

        }


        static public void setCrashlyticsParam(string paramName, string paramValue)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitSetCrashlyticsParam(paramName, paramValue);
        }

        static public void trackFacebookEvent(string eventName, string eventParamString)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitEventTrackFacebook(eventName, eventParamString);
        }

        static public void trackFacebookPurchaseEvent(float valueToSum, string currencyCode)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitEventTrackFacebookPurchaseEvent(valueToSum, currencyCode);
        }

        static public void trackFullFacebookEvent(string logEvent, float? valueToSum = default(float?), Dictionary<string, object> parameters = null)
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            try
            {
                var strongValue = (float)(valueToSum ?? 0);

                var dictJspn = ConvertDictionaryToJSPN(parameters);

                sayKitEventTrackFullFacebook(logEvent, strongValue, dictJspn);
            }
            catch (Exception ex)
            {
                SayKitDebug.Log("trackFullFacebookEvent:" + ex.Message);
            }

        }

        static private string ConvertDictionaryToJSPN(Dictionary<string, object> parameters)
        {
            var dictJspn = "";

            try
            {
                if (parameters != null)
                {

                    foreach (var item in parameters)
                    {
                        if (item.Value.GetType() == typeof(string))
                        {
                            dictJspn += item.Key + "%|%" + item.Value + "%|%" + "string" + "}&%&{";
                        }
                        else if (item.Value.GetType() == typeof(bool))
                        {
                            dictJspn += item.Key + "%|%" + item.Value + "%|%" + "bool" + "}&%&{";
                        }
                        else if (item.Value.GetType() == typeof(int) ||
                                    item.Value.GetType() == typeof(long))
                        {
                            dictJspn += item.Key + "%|%" + item.Value + "%|%" + "int" + "}&%&{";
                        }
                        else if (item.Value.GetType() == typeof(float) ||
                                  item.Value.GetType() == typeof(double))
                        {
                            dictJspn += item.Key + "%|%" + item.Value + "%|%" + "float" + "}&%&{";
                        }
                        else
                        {
                            dictJspn += item.Key + "%|%" + JsonUtility.ToJson(item.Value) + "%|%" + "json" + "}&%&{";
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                SayKitDebug.Log("ConvertDictionaryToJSPN:" + ex.Message);
            }

            return dictJspn;
        }


        static public bool isFacebookSDKInitialized()
        {

#if UNITY_ANDROID
            try
            {
                var fcebookSDK = new AndroidJavaClass("com.facebook.FacebookSdk");
                var name = fcebookSDK.CallStatic<bool>("isInitialized");

                return name;
            }
            catch (System.Exception ex)
            {
                SayKitDebug.Log(ex.Message);

                return false;
            }
#else
            return true;
#endif
        }


        static public void trackEventWithoutInit(string eventName, string eventParamString)
        {

            var segment = SayKit.remoteConfig.runtime.segment;
            if (SayKit.config.overrideAnalyticSegment >= 0)
            {
                segment = SayKit.config.overrideAnalyticSegment;
            }

            sayKitEventTrackFull(
                SayKit.config.appKey,
                SayKit.runtimeInfo.idfa,
                SayKit.runtimeInfo.idfv,
                SystemInfo.operatingSystem,
                SystemInfo.deviceModel,
                Application.version,
                segment,
                eventName,
                0,
                0,
                eventParamString,
                0,
                0,
                "",
                "",
                Level,
                SCurrency,
                HCurrency
            );
        }

#if SAYKIT_PURCHASING

        public static void trackPurchase(PurchaseEventArgs purchaseEventArgs)
        {
            try
            {
                trackPurchase(purchaseEventArgs.purchasedProduct);
            }
            catch (System.Exception ex)
            {
                SayKitDebug.Log(ex.Message);
            }
        }

        public static void trackPurchase(Product purchasedProduct)
        {

            try
            {

                var price = purchasedProduct.metadata.localizedPrice;
                float lPrice = decimal.ToSingle(price);
                var currencyCode = purchasedProduct.metadata.isoCurrencyCode;

                var wrapper = (Dictionary<string, object>)UnityEngine.Purchasing.MiniJson.JsonDecode(purchasedProduct.receipt);
                if (null == wrapper)
                {
                    return;
                }

                var payload = (string)wrapper["Payload"]; // For Apple this will be the base64 encoded ASN.1 receipt
                var productId = purchasedProduct.definition.id;
                
                var eventExtra = new Dictionary<string, object>();
                eventExtra["product_id"] = productId;
                eventExtra["currency"] = currencyCode;
                eventExtra["price"] = lPrice;


                InAppItem inAppItem = new InAppItem
                {
                    price = lPrice,
                    product_id = productId,
                    currency = currencyCode
                };

                var transactionId = purchasedProduct.transactionID;

#if UNITY_ANDROID

                var gpDetails = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                var gpJson = (string)gpDetails["json"];
                var gpSig = (string)gpDetails["signature"];
                AnalyticsTenjin.completedAndroidPurchase(productId, currencyCode, 1, lPrice, gpJson, gpSig);

                eventExtra["json"] = gpJson;
                eventExtra["signature"] = gpSig;
                trackEvent("iap_android", UnityEngine.Purchasing.MiniJson.JsonEncode(eventExtra));

                inAppItem.json = gpJson;
                inAppItem.signature = gpSig;

                inAppItem.transaction_id = transactionId;

#elif UNITY_IOS
                AnalyticsTenjin.completedIosPurchase(productId, currencyCode, 1, lPrice , transactionId, payload);

                eventExtra["transaction_id"] = transactionId;
                eventExtra["receipt"] = payload;
                trackEvent("iap_ios", UnityEngine.Purchasing.MiniJson.JsonEncode(eventExtra));

                inAppItem.receipt = payload;
                inAppItem.transaction_id = transactionId;

                ConversionManager.Instance.RunRequestConversionData("iap");
#endif

                if (SayKit.config.inAppPurchaseServerCheck)
                {
                    InAppManager.Instance.CheckInApp(inAppItem);
                }

            }
            catch (System.Exception ex)
            {
                SayKitDebug.Log(ex.Message);
            }
        }

#endif // SAYKIT_PURCHASING

        public static void trackCrashes()
        {
            var reports = CrashReport.reports;
            foreach (var r in reports)
            {
                SayKitDebug.Log("tracking crash " + r.time);
                AnalyticsEvent.trackEvent("crash_report", r.text);
                r.Remove();
            }
        }

        static bool totalMemoryTracked = false;
        static public void trackAvailableMemory()
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

#if UNITY_ANDROID
            if (!totalMemoryTracked)
            {
                totalMemoryTracked = true;
                sayKitEventTrackTotalMemory();
            }

            sayKitEventTrackAvailableMemory();
#endif
        }


        static public void trackSDKVersions()
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitTrackSDKVersions();
        }

        static public void trackFirebaseId()
        {
            if (!RuntimeInfoManager.initialized)
            {
                return;
            }

            sayKitTrackFirebaseId();
        }


        public static void overrideTrackLevel(int level)
        {
            AnalyticsEvent.Level = level;
        }
        
    }
}