using UnityEngine;

using UnityEngine.SceneManagement;

#if SAYKIT_PURCHASING
using UnityEngine.Purchasing;
#endif

using System;
using System.Collections;

using SayKitInternal;
using System.Collections.Generic;


public class SayKitConfig {
    public string appKey = "";
    public string bannerAdUnitId = "";
    public string interstitialAdUnitId = "";
    public string rewardedAdUnitId = "";

    public string tenjinApiKey = "JXK4EQ6YOSESVJ9FBPM9QAZKZKEZGJSC";

    /// <summary>
    /// Turn on to send purchase events to Facebook and Firebase.
    /// Important! This flag will disable Facebook event auto-tracking on Android.
    /// </summary>
    public bool inAppPurchaseServerCheck = false;

    public bool attributionConfigUpdate = false;
    public Action remoteConfigUpdated;

    public SayKitLanguage overrideSystemLanguage = SayKitLanguage.English;
    public int overrideAnalyticSegment = -1;

    /// <summary>
    /// Call Notification request manually. Make sure you are calling notification request every session.
    /// </summary>
    public bool customNotificationRequest = false;
}


public class SayKit : MonoBehaviour {

    
    const int version = 2021091400;
    static public int GetVersion { get { return version; } }
    static public string BuildVersion { get { return PlatformManager.getBuildVersion(); } }


    enum InitState { None, Platform, RemoteConfig, IDFA, Gdpr, Done }

    static private SayKitConfig _config = new SayKitConfig();
    static public SayKitConfig config { get { return _config; } }
    static private RuntimeInfo _runtimeInfo = new RuntimeInfo();
    static public RuntimeInfo runtimeInfo { get { return _runtimeInfo; } }
    static private RemoteConfig _remoteConfig = new RemoteConfig();
    static public RemoteConfig remoteConfig { get { return _remoteConfig; } }
    static public SayKitGameConfig gameConfig { get { return _remoteConfig.game_settings; } }

    static private SayKit instance;
    static public SayKit GetInstance() { return instance; } 

    static private InitState initState = InitState.None;
    
    static public bool isInitialized { get { return initState == InitState.Done; } }
    static public float initializedProgress
    {
        get
        {
            switch (initState)
            {
                case InitState.Platform:
                    return 0.20f;
                case InitState.RemoteConfig:
                    return 0.40f;
                case InitState.IDFA:
                    return 0.60f;
                case InitState.Gdpr:
                    return 0.80f;
                case InitState.Done:
                    return 1f;
                default:
                    return 0f;
            }
        }
    }
    
    static public void init(SayKitConfig withConfig) {


        SayKitDebug.InitDebugLogs();

        if (initState != InitState.None) {
            SayKitDebug.Log("SayKit init was started earlier.");
            return;
        }
        initState = InitState.Platform;

        _config = withConfig;


        ImpressionLogManager.Instance.Init();
        SayKitBridje.Initialize();
       
        // Try to preload local configs
        RemoteConfigManager.initLocal(withConfig.appKey, Application.version);
        _remoteConfig = RemoteConfigManager.config;
        
        GameObject sayKitObject = new GameObject ("[SayKit]");
		DontDestroyOnLoad (sayKitObject);
        instance = sayKitObject.AddComponent<SayKit> ();

        instance.StartCoroutine(InternetReachabilityService.CheckInternetConnection());
        instance.StartCoroutine(RuntimeInfoManager.initRoutine());
        instance.StartCoroutine(initRoutine());

		if (SayKit.isPremium == false)
		{
			SayKit.showBanner();
		}
    }

	static IEnumerator initRoutine() {
        while (true) {
            switch (initState) {
                case InitState.Platform:
                    
                    if (SayKitInternal.RuntimeInfoManager.initialized) {
                        _runtimeInfo = RuntimeInfoManager.runtimeInfo;
                        initState = InitState.RemoteConfig;
                        instance.StartCoroutine(RemoteConfigManager.initRoutine());
                    } else {
                        SayKitDebug.Log("Waiting for RuntimeInfoManager");
                    }
                    break;

                case InitState.RemoteConfig:
                    
                    if (SayKitInternal.RemoteConfigManager.initialized) {
                        initializeRemoteConfig();

                        RuntimeInfoManager.InitializeIDFA();

                        initState = InitState.IDFA;
                    } else {
                        SayKitDebug.Log("Waiting for RemoteConfigManager");
                    }
                    break;

                case InitState.IDFA:
                    
                    if (RuntimeInfoManager.IDFAInitialized)
                    {
                        instance.StartCoroutine(AdsManager.initRoutine());
                        string appVersion = SayKit.remoteConfig.runtime.segment == 0 ? SayKit.runtimeInfo.version : SayKit.runtimeInfo.version + "." + SayKit.remoteConfig.runtime.segment;
                        AnalyticsEvent.init(config.appKey, version.ToString(), appVersion, _remoteConfig.ads_settings.track_waterfall);
                        AnalyticsEvent.trackEvent("app_start", version);
                        AnalyticsEvent.trackEvent("language", runtimeInfo.language);
                        AnalyticsEvent.trackEvent("unity_engine", Application.unityVersion);
                        //AnalyticsEvent.trackEvent("skad_status", PlatformManager.sayKitGetCurrentIDFAStatus());

                        AnalyticsEvent.trackCrashes();

#if SAYKIT_NOTIFICATIONS && !UNITY_CLOUD_BUILD
                        if (!_config.customNotificationRequest)
                        {
                            instance.StartCoroutine(NotificationManager.initRoutine());
                        }
#endif
                        initState = InitState.Gdpr;
                    }
                    break;

                case InitState.Gdpr:

                    if (!AdsManager.initialized)
                    {
                        break;
                    }

                    // initialized
                    initState = InitState.Done;
                    instance.StartCoroutine(AdsManager.fetchRoutine());
                    instance.StartCoroutine(FpsCounter.reportFpsRoutine());
                    instance.StartCoroutine(DebugFpsCounter.StartDebugFPSRoutine());

                    SayPromo.init();


                    PlatformManager.initAfterGdpr(config.inAppPurchaseServerCheck);
                    AttributionManager.RunAttribution();

#if UNITY_IOS
                    ConversionManager.Instance.Init();
                    instance.StartCoroutine(ConversionManager.Instance.startConversionRoutine());
#endif

                    AnalyticsEvent.trackSayKitLoadDuration();
                    AnalyticsEvent.trackSDKVersions();
                    AnalyticsEvent.trackFirebaseId();

                    if (config.inAppPurchaseServerCheck)
                    {
                        InAppManager.Instance.Init();
                        instance.StartCoroutine(InAppManager.Instance.AnalyzeInAppData());
                    }

                    break;

                case InitState.Done:
                    
                    yield break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // Ads
    static public void showBanner() {
#if UNITY_EDITOR
        return;
#else
        AdsManager.showBanner();
#endif
    }
    static public void hideBanner() {
#if UNITY_EDITOR
        return;
#else
        AdsManager.hideBanner();
#endif
    }

    static public bool isInterstitialAvailable(string place) {
#if UNITY_EDITOR
        return true;
#else
        return AdsManager.isInterstitialAvailable(place);
#endif
    }

    static public bool isRewardedAvailable(string place)
    {
#if UNITY_EDITOR
        return true;
#else
        return AdsManager.isRewardedAvailable(place);
#endif
    }

    static public bool showInterstitial(string place, Action onCloseCallback = null) {

#if UNITY_EDITOR
        onCloseCallback?.Invoke();

        return true;
#else
        return AdsManager.showInterstitial(place, onCloseCallback);
#endif

    }

    static public void showRewarded(string place, Action<bool> onCloseCallback) {
#if UNITY_EDITOR
        onCloseCallback?.Invoke(true);
#else
        AdsManager.showRewarded(place, onCloseCallback);
#endif
    }


    static public void trackInterstitialOffer(string place){
        AnalyticsEvent.trackInterstitialOffer(place);
    }

    static public void trackRewardedOffer(string place) {
        AnalyticsEvent.trackRewardedOffer(place);
    }


    


    //Facebook
    public static void logFBAppEvent(string logEvent, float? valueToSum = default(float?), Dictionary<string, object> parameters = null)
    {
        AnalyticsEvent.trackFullFacebookEvent(logEvent, valueToSum, parameters);
    }


    public static void setCrashlyticsParam(string paramName, string paramValue)
    {
        if (paramName?.Length > 0 && paramValue?.Length > 0)
        {
            AnalyticsEvent.setCrashlyticsParam(paramName, paramValue);
        }
    }


    public static void logFirebaseEvent(string eventName, string extraParam = "")
    {
        if (extraParam == null)
        {
            extraParam = "";
        }

        if (eventName?.Length > 0)
        {
            AnalyticsEvent.trackFirebaseEvent(eventName, extraParam);
        }
    }

    public static void logFirebaseEvent(string eventName, string paramName, object paramValue)
    {
        if (paramName?.Length > 0 && paramValue != null)
        {
            if (eventName?.Length > 0)
            {
                logFirebaseEvent(eventName,
                0,
                new Dictionary<string, object>
                {
                { paramName, paramValue }
                }
                );
            }
        }
        else
        {
            logFirebaseEvent(eventName, "");
        }
    }

    public static void logFirebaseEvent(string eventName, float? valueToSum = default(float?), Dictionary<string, object> parameters = null)
    {
        if (eventName?.Length > 0)
        {
            AnalyticsEvent.trackFullFirebaseEvent(eventName, valueToSum, parameters);
        }
    }


    public static bool isFacebookSDKInitialized()
    {
        return AnalyticsEvent.isFacebookSDKInitialized();
    }


    // Events
    static public void trackLevelStarted(int level)
    {
        AnalyticsEvent.trackLevelStarted("", level);
    }
    static public void trackLevelStarted(string tag, int level)
    {
        AnalyticsEvent.trackLevelStarted(tag, level);
    }
    static public void trackLevelStarted(string tag, int level, int score, string extra)
    {
        AnalyticsEvent.trackLevelStarted(tag, level, score, extra);
    }
    static public void trackLevelStarted(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
    {
        AnalyticsEvent.trackLevelStarted(tag, level, score, extra, eventParam1, eventParam2);
    }

    static public void trackLevelCompleted(int level, int score)
    {
        AnalyticsEvent.trackLevelCompleted("", level, score);
    }
    static public void trackLevelCompleted(string tag, int level, int score)
    {
        AnalyticsEvent.trackLevelCompleted(tag, level, score);
    }
    static public void trackLevelCompleted(string tag, int level, int score, string extra)
    {
        AnalyticsEvent.trackLevelCompleted(tag, level, score, extra);
    }
    static public void trackLevelCompleted(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
    {
        AnalyticsEvent.trackLevelCompleted(tag, level, score, extra, eventParam1, eventParam2);
    }

    static public void trackLevelFailed(int level, int score)
    {
        AnalyticsEvent.trackLevelFailed("", level, score);
    }
    static public void trackLevelFailed(string tag, int level, int score)
    {
        AnalyticsEvent.trackLevelFailed(tag, level, score);
    }
    static public void trackLevelFailed(string tag, int level, int score, string extra)
    {
        AnalyticsEvent.trackLevelFailed(tag, level, score, extra);
    }
    static public void trackLevelFailed(string tag, int level, int score, string extra, int eventParam1, string eventParam2)
    {
        AnalyticsEvent.trackLevelFailed(tag, level, score, extra, eventParam1, eventParam2);
    }



    static public void trackLevelExtraStarted()
    {
        AnalyticsEvent.trackLevelExtraStarted("");
    }
    static public void trackLevelExtraStarted(string tag)
    {
        AnalyticsEvent.trackLevelExtraStarted(tag);
    }

    static public void trackLevelExtraCompleted(int score)
    {
        AnalyticsEvent.trackLevelExtraCompleted("", score);
    }
    static public void trackLevelExtraCompleted(string tag, int score)
    {
        AnalyticsEvent.trackLevelExtraCompleted(tag, score);
    }

    static public void trackLevelExtraFailed(int score)
    {
        AnalyticsEvent.trackLevelExtraFailed("", score);
    }
    static public void trackLevelExtraFailed(string tag, int score)
    {
        AnalyticsEvent.trackLevelExtraFailed(tag, score);
    }




    static public void trackLevelStageStarted(int number)
    {
        AnalyticsEvent.trackStageStarted("", number);
    }
    static public void trackLevelStageStarted(string tag, int number)
    {
        AnalyticsEvent.trackStageStarted(tag, number);
    }

    static public void trackLevelStageCompleted(int number, int score)
    {
        AnalyticsEvent.trackStageCompleted("", number, score);
    }
    static public void trackLevelStageCompleted(string tag, int number, int score)
    {
        AnalyticsEvent.trackStageCompleted(tag, number, score);
    }

    static public void trackLevelStageFailed(int number, int score)
    {
        AnalyticsEvent.trackStageFailed("", number, score);
    }
    static public void trackLevelStageFailed(string tag, int number, int score)
    {
        AnalyticsEvent.trackStageFailed(tag, number, score);
    }



    static public void trackChunkStarted(string name, int sequenceNumber)
    {
        AnalyticsEvent.trackChunkStarted(name, sequenceNumber);
    }
    static public void trackChunkStarted(string name, int sequenceNumber, Dictionary<string, object> customData)
    {
        AnalyticsEvent.trackChunkStarted(name, sequenceNumber, customData);
    }

    static public void trackChunkCompleted()
    {
        AnalyticsEvent.trackChunkCompleted();
    }
    static public void trackChunkCompleted(Dictionary<string, object> customData)
    {
        AnalyticsEvent.trackChunkCompleted(customData);
    }

    static public void trackChunkFailed()
    {
        AnalyticsEvent.trackChunkFailed();
    }
    static public void trackChunkFailed(Dictionary<string, object> customData)
    {
        AnalyticsEvent.trackChunkFailed(customData);
    }


    static public void trackTutorialStep(string tutorialName, string stepName)
    {
        AnalyticsEvent.trackTutorialStep(tutorialName, stepName);
    }


    static public void trackEvent(string eventName) {
        AnalyticsEvent.trackEvent(eventName);
    }
    static public void trackEvent(string eventName, int eventParam) {
        AnalyticsEvent.trackEvent(eventName, eventParam);
    }
    static public void trackEvent(string eventName, int eventParam1, int eventParam2) {
        AnalyticsEvent.trackEvent(eventName, eventParam1, eventParam2);
    }
    static public void trackEvent(string eventName, int eventParam1, int eventParam2, string eventParam) {
        AnalyticsEvent.trackEvent(eventName, eventParam1, eventParam2, eventParam);
    }

    static public void trackEvent(string eventName, int eventParam1, int eventParam2, int eventParam3, string eventParam = "") {
        AnalyticsEvent.trackEvent(eventName, eventParam1, eventParam2, eventParam, eventParam3);
    }

    static public void trackEvent(string eventName, string eventParam) {
        AnalyticsEvent.trackEvent(eventName, eventParam);
    }
    static public void trackEvent(string eventName, string eventParam, string eventParam2) {
        AnalyticsEvent.trackEvent(eventName, eventParam, eventParam2);
    }
    
    static public void trackEvent(string eventName, int eventParam1, string eventParam2, string eventParam3) {
        AnalyticsEvent.trackEvent(eventName,eventParam1,0,eventParam2,eventParamString2:eventParam3);
    }

    static public void trackEvent(string eventName, int eventParam1, int eventParam2, string eventParam3, string eventParam4) {
        AnalyticsEvent.trackEvent(eventName, eventParam1, eventParam2, eventParam3, 0, 0, eventParam4);
    }

    static public void trackTagEvent(string eventName, string tag, int eventParam1, int eventParam2, string eventParam3, string eventParam4) {
        AnalyticsEvent.trackTagEvent(eventName, tag, eventParam1, eventParam2, 0, eventParam3, eventParam4);
    }



    static public void trackItem(string item) {
        AnalyticsEvent.trackItem(item);
    }

    static public void trackItem(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
    {
        AnalyticsEvent.trackItem(item, ownedItems, sourceId, customData);
    }

    static public void trackItemLoss(string item, int ownedItems, SourceType sourceId, Dictionary<string, object> customData)
    {
        AnalyticsEvent.trackItemLoss(item, ownedItems, sourceId, customData);
    }


    static public void trackScreen(string screen)
	{
		AnalyticsEvent.trackEvent("screen", screen);
        DebugFpsCounter.UpdateScreen(screen);

        // event = "screen"
        // extra = screen
    }

	static public void trackClick(string screen, string element)
	{
		// event = "click"
		// extra = screen + ":" + element

		var extra = element;
		
		if (screen != "")
		{
			extra = screen + ":" + element;
		}

		AnalyticsEvent.trackEvent("click", extra);
	}


	static public void trackSoftIncome(int amount, int total)
	{
		AnalyticsEvent.trackSoftIncome("soft_income", amount, total);
	}

	static public void trackSoftOutcome(int amount, int total)
	{
		AnalyticsEvent.trackSoftOutcome("soft_outcome", amount, total);
	}

	static public void trackHardIncome(int amount, int total)
	{
		AnalyticsEvent.trackHardIncome("hard_income", amount, total);
	}

	static public void trackHardOutcome(int amount, int total)
	{
		AnalyticsEvent.trackHardOutcome("hard_outcome", amount, total);
	}

    static public void trackSoftIncome(int amount, int total, string place)
    {
        AnalyticsEvent.trackSoftIncome("soft_income", amount, total, place);
    }

    static public void trackSoftOutcome(int amount, int total, string place)
    {
        AnalyticsEvent.trackSoftOutcome("soft_outcome", amount, total, place);
    }

    static public void trackHardIncome(int amount, int total, string place)
    {
        AnalyticsEvent.trackHardIncome("hard_income", amount, total, place);
    }

    static public void trackHardOutcome(int amount, int total, string place)
    {
        AnalyticsEvent.trackHardOutcome("hard_outcome", amount, total, place);
    }


    static public void trackAplicationLoaded()
    {
        AnalyticsEvent.trackApplicationLoadDuration();
    }

    static public void trackEventWithoutInit(string eventName, string eventParamString)
    {
        AnalyticsEvent.trackEventWithoutInit(eventName, eventParamString);
    }

#if SAYKIT_PURCHASING
    static public void trackPurchase(PurchaseEventArgs purchaseEventArgs) {
        AnalyticsEvent.trackPurchase(purchaseEventArgs);
    }

    static public void trackPurchase(Product purchasedProduct){
        AnalyticsEvent.trackPurchase(purchasedProduct);
    }
#endif

    // In-App Purchases

    static public void enablePremium() {
        Storage.instance.isPremium = true;
        Storage.instance.save();
        hideBanner();
        SayKit.trackEvent("premium_enabled");
    }

    static public void disablePremium()
    {
        Storage.instance.isPremium = false;
        Storage.instance.save();
        showBanner();
        SayKit.trackEvent("premium_disabled");
    }


    static public bool isPremium { get { return Storage.instance.isPremium; }}

    // Rate App
    static public bool showRateAppPopup() {
        return PlatformManager.showRateAppPopup();
    }

    static public bool showCustomRateAppPopup() {
        return PlatformManager.showCustomRateAppPopup();
    }

    // GDPR
    static public void grantGdprConsent() {
        trackEvent("gdpr_grant_consent");
        AdsManager.grantGdprConsent();
    }
    static public void revokeGdprConsent() {
        trackEvent("gdpr_revoke_consent");
        AdsManager.revokeGdprConsent();
    }
    static public bool? isGdprApplicable() {
        return AdsManager.isGdprApplicable(); 
    }

    static public void RequestNotificationToken()
    {
#if SAYKIT_NOTIFICATIONS && !UNITY_CLOUD_BUILD
        if (_config.customNotificationRequest)
        {
            instance.StartCoroutine(NotificationManager.initRoutine());
        }
#endif
    }

    static public string getLocalizedString(string key, string val1 = null, string val2 = null, string val3 = null, string val4 = null, string val5 = null) {
        var tuple = remoteConfig.getLocalizedMessage(key, val1, val2, val3, val4, val5);
        return tuple.Item1;
    }

    static public (string, bool) getLocalizedTuple(string key, string val1 = null, string val2 = null, string val3 = null, string val4 = null, string val5 = null)
    {
        return remoteConfig.getLocalizedMessage(key, val1, val2, val3, val4, val5);
    }

    static public bool hasLocalizedMessage(string key) {
        return remoteConfig.hasLocalizedMessage(key);
    }

    static internal void initializeRemoteConfig() {
        _remoteConfig = RemoteConfigManager.config;
    }

    static public SayKitLanguage getCurrentLanguage()
    {
        return PlatformManager.CurrentLanguage;
    }

    /// <summary>
    /// DEBUG only method!
    /// </summary>
    /// <param name="language"></param>
    static public void changeLanguage(SayKitLanguage language)
    {
        remoteConfig.setupLocalization(language);
    }

    /// <summary>
    /// DEBUG only method!
    /// </summary>
    static public void overrideTrackLevel(int level)
    {
        AnalyticsEvent.overrideTrackLevel(level);
    }

    
    static public void requestConfigMigration(string sourceVersion)
    {
        instance.StartCoroutine(RemoteConfigManager.RequestConfigMigration(sourceVersion));
    }


    void OnApplicationPause(bool pauseStatus)
    {
        AttributionManager.OnApplicationPause(pauseStatus);
        RuntimeInfoManager.OnApplicationPause(pauseStatus);
    }


    void Update()
    {
        if (isInitialized)
        {
            FpsCounter.update();
            DebugFpsCounter.Update();
        }
    }


    void OnEnable()
    {
        Application.logMessageReceived += SayKitDebug.LogCallback;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= SayKitDebug.LogCallback;
    }

}
