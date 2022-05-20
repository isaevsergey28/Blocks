using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SayKitInternal {

    class AdsManager
    {
        private static bool _useMaxMed = true;//
        private static bool _maxsdkInitialized = false;

        private static string bannerAdUnitId;
        private static string interstitialAdUnitId;
        private static string rewardedAdUnitId;

        public static bool initialized = false;

        private static bool shouldShowBannerAfterInit = false;
        private static bool bannerIsShowing = false;
        private static bool bannerWasCreated = false;

        private static float bannerLoadDelay = 10f;
        private static float gdprLoadDelay = 5f;

        private static readonly string SAYKIT_GDPR_TIMESTAMP = "SAYKIT_GDPR_TIMESTAMP";
        private static int _migrateGDPRTimestamp = 1584621000;



        private static bool isNeedToShowGDPRPopUp()
        {
            var serverTimestamp = SayKit.remoteConfig.runtime.gdpr_applicable;

            var localTimestamp = PlayerPrefs.GetInt(SAYKIT_GDPR_TIMESTAMP);
            if (localTimestamp == 0 && Storage.instance.consetWasGranted)
            {
                localTimestamp = _migrateGDPRTimestamp;
                PlayerPrefs.SetInt(SAYKIT_GDPR_TIMESTAMP, _migrateGDPRTimestamp);
            }

            if (serverTimestamp > localTimestamp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool MaxSDKShouldShowGdprPopup()
        {
            var consentState = MaxSdk.GetSdkConfiguration().ConsentDialogState;

            if (consentState == MaxSdkBase.ConsentDialogState.Applies)
            {
                // Show user consent dialog
                return true;
            }
            else if (consentState == MaxSdkBase.ConsentDialogState.DoesNotApply)
            {
                return false;
            }
            else
            {
                // Consent dialog state is unknown. Proceed with initialization, but check if the consent
                // dialog should be shown on the next application initialization
                return true;
            }
        }

        private static bool shouldShowGdprPopup()
        {
            bool? shouldShow = false;
            if (_useMaxMed)
            {
                shouldShow = MaxSDKShouldShowGdprPopup();
            }
            
            return shouldShow.GetValueOrDefault(false);
        }


        public static bool isGdprApplicable()
        {

            if (SayKit.remoteConfig.runtime.status == 0)
            {
                return false;
            }

            return SayKit.remoteConfig.runtime.gdpr_applicable != 0;
        }

        public static void grantGdprConsent()
        {
            Storage.instance.consetWasGranted = true;
            Storage.instance.save();

            if (_useMaxMed)
            {
                MaxSdk.SetHasUserConsent(true);
            }
            
        }

        public static void revokeGdprConsent()
        {
            if (SayKit.remoteConfig.runtime.status == 0)
            {
                return;
            }

            Storage.instance.consetWasGranted = false;
            Storage.instance.save();

            PlayerPrefs.SetInt(SAYKIT_GDPR_TIMESTAMP, 0);

            if (_useMaxMed)
            {
                MaxSdk.SetHasUserConsent(false);
            }
            //else
            //{
            //    SayMed.RevokeGdprConsent();
            //}
            
            SayKitUI.ShowPopup();
        }



        public static IEnumerator initRoutine()
        {
            if (_useMaxMed)
            {
                Debug.Log("MaxSDK selected!");
                //Android
                //bannerAdUnitId = "b73ddee17a9d2dd1";
                //interstitialAdUnitId = "cac365a2e1e7252b";
                //rewardedAdUnitId = "1120e279461799f6";

                //iOS
                //bannerAdUnitId = "dcb3bee467986f83";
                //interstitialAdUnitId = "acf709eb6ab8f132";
                //rewardedAdUnitId = "74772d1484eadf34";


                bannerAdUnitId = SayKit.remoteConfig.ads_settings.maxsdk_banner_id;
                interstitialAdUnitId = SayKit.remoteConfig.ads_settings.maxsdk_interstitial_id;
                rewardedAdUnitId = SayKit.remoteConfig.ads_settings.maxsdk_rewarded_id;
            }
            else
            {
                Debug.Log("SayMed selected!");

                bannerAdUnitId = SayKit.remoteConfig.ads_settings.saymed_banner_id;
                interstitialAdUnitId = SayKit.remoteConfig.ads_settings.saymed_interstitial_id;
                rewardedAdUnitId = SayKit.remoteConfig.ads_settings.saymed_rewarded_id;
            }

            // override
            if (SayKit.config.bannerAdUnitId != "")
            {
                bannerAdUnitId = SayKit.config.bannerAdUnitId;
            }
            if (SayKit.config.interstitialAdUnitId != "")
            {
                interstitialAdUnitId = SayKit.config.interstitialAdUnitId;
            }
            if (SayKit.config.rewardedAdUnitId != "")
            {
                rewardedAdUnitId = SayKit.config.rewardedAdUnitId;
            }


            yield return new WaitUntil(() => InternetReachabilityService.IsInternetStatusChecked);

            if (InternetReachabilityService.InternetReachability == NetworkReachability.NotReachable)
            {
                // Apple rejects GDPR on iOS.
#if UNITY_ANDROID
                if (SayKit.remoteConfig.runtime.status == 1 && Storage.instance.consetWasGranted == false)
                {
                    SayKitUI.ShowPopup();
                    yield return new WaitUntil(() => Storage.instance.consetWasGranted == true);

                    PlayerPrefs.SetInt(SAYKIT_GDPR_TIMESTAMP, Utils.currentTimestamp);
                }
#endif
            }
            else
            {
                SayKitDebug.Log("GDPR timestamp: " + SayKit.remoteConfig.runtime.gdpr_applicable);

                if (SayKit.remoteConfig.runtime.status == 1 && isGdprApplicable())
                {
                    SayKitDebug.Log("GDPR status is clear");

                    if (!isNeedToShowGDPRPopUp() && Storage.instance.consetWasGranted)
                    {
                        grantGdprConsent();
                    }
                    else
                    {
                        SayKitUI.ShowPopup();
                        yield return new WaitUntil(() => Storage.instance.consetWasGranted == true);

                        PlayerPrefs.SetInt(SAYKIT_GDPR_TIMESTAMP, Utils.currentTimestamp);
                    }
                }
            }

            string appVersion = SayKit.remoteConfig.runtime.segment == 0 ? SayKit.runtimeInfo.version : SayKit.runtimeInfo.version + "." + SayKit.remoteConfig.runtime.segment;

            if (_useMaxMed)
            {
                MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                    // AppLovin SDK is initialized, start loading ads
                    
                    _maxsdkInitialized = true;

                    if (Storage.instance.consetWasGranted)
                    {
                        MaxSdk.SetHasUserConsent(true);
                    }

                    if (SayKit.remoteConfig.runtime.debug_mediation == 1)
                    {
                        MaxSdk.ShowMediationDebugger();
                        MaxSdk.SetCreativeDebuggerEnabled(true);
                    }
                };

                SayKit.setCrashlyticsParam("mediation", "maxsdk");
                SayKit.setCrashlyticsParam("maxsdk_version", MaxSdk.Version.ToString());

                SubscribeMaxSDKEvents();


                //MaxSdk.SetIsAgeRestrictedUser(true);
                //MaxSdk.SetDoNotSell(false);

                MaxSdk.SetSdkKey(SayKit.remoteConfig.ads_settings.maxsdk_key);  
                //Debug.Log("MaxSDK key: " + SayKit.remoteConfig.ads_settings.maxsdk_key);
                MaxSdk.SetUserId(SayKit.runtimeInfo.idfv);

                MaxSdk.InitializeSdk();

                yield return new WaitUntil(() => _maxsdkInitialized);


                startedAt = Utils.currentTimestamp;
                yield return initAfterGdprRoutine();
            }
            
        }

        public static IEnumerator initAfterGdprRoutine()
        {
            initialized = true;

            fetchInterstitial();
            fetchRewarded();

            if (shouldShowBannerAfterInit)
            {
                yield return new WaitUntil(() => Time.time >= bannerLoadDelay);
                yield return new WaitUntil(() => Storage.instance.rateAppPopupWasShowed);

                if (shouldShowBannerAfterInit)
                {
#if UNITY_EDITOR
                    // ignore
#elif !SAYKIT_BANNER_DISABLED
                    showBanner();
#endif
                }
            }

            yield break;
        }


        private static void SubscribeMaxSDKEvents()
        {
            // Attach callback
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnMaxBannerShown;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnMaxBannerClicked;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnMaxBannerRevenuePaid;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnMaxInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnMaxInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnMaxInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnMaxInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnMaxInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnMaxInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnMaxInterstitialRevenuePaid;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnMaxRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnMaxRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnMaxRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnMaxRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnMaxRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnMaxRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnMaxRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnMaxRewardedAdRevenuePaidEvent;
        }


        private static string GetBannerAdUnitId()
        {
#if SAYKIT_BANNER_DISABLED
            return null;
#else
            return bannerAdUnitId;
#endif
        }

        public static void showBanner()
        {

#if !SAYKIT_BANNER_DISABLED

            if (!initialized || Time.time < bannerLoadDelay)
            {
                shouldShowBannerAfterInit = true;
                return;
            }

            if (SayKit.remoteConfig.ads_settings.banner_disabled == 1) return;
            if (SayKit.isPremium) return;
            if (string.IsNullOrEmpty(GetBannerAdUnitId())) return;

            if (!bannerWasCreated)
            {
                if (_useMaxMed)
                {
                    MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
                    //MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);
                }
                

                bannerWasCreated = true;
            }

            if (!bannerIsShowing)
            {
                bannerIsShowing = true;
                SayKitDebug.Log("Showing banner for unit " + bannerAdUnitId);

                if (_useMaxMed)
                {
                    MaxSdk.ShowBanner(bannerAdUnitId);
                }
               
            }
#endif
        }

        public static void hideBanner()
        {
#if !SAYKIT_BANNER_DISABLED

            shouldShowBannerAfterInit = false;

            if (bannerWasCreated && bannerIsShowing)
            {
                bannerIsShowing = false;

                if (_useMaxMed)
                {
                    MaxSdk.HideBanner(bannerAdUnitId);
                }
                

                SayKitBanner.Hide();
            }
#endif
        }



        internal static bool hasInterstitial(bool useCachedStatus = true)
        {
            if (useCachedStatus)
            {
                return hasInterstitialCachedStatus;
            }

            if (_useMaxMed)
            {
                return MaxSdk.IsInterstitialReady(interstitialAdUnitId);
            }

            return false;
            
        }

        private static void fetchInterstitialCall()
        {
            if (_useMaxMed)
            {
                MaxSdk.LoadInterstitial(interstitialAdUnitId);
                if (MaxSdk.IsInterstitialReady(interstitialAdUnitId))
                {
                    hasInterstitialCachedStatus = true;
                }
            }
        }

        private static void fetchInterstitial()
        {
            hasInterstitialCachedStatus = false;
            shouldFetchIntersitital = true;

            if (_useMaxMed)
            {
                if (MaxSdk.IsInterstitialReady(interstitialAdUnitId))
                {
                    hasInterstitialCachedStatus = true;
                }
            }
            
        }


        public static bool isInterstitialAvailable(string place)
        {
            if (!initialized)
            {
                return false;
            }

            if (place == null)
            {
                place = "ad_interstitial_null";
            }
            else if (place.Equals(""))
            {
                place = "ad_interstitial_empty";
            }

            if (SayKit.isPremium)
            {
                return false;
            }

            if (shouldSkipPlace(place, isRewarded: false))
            {
                return false;
            }

            if (!hasInterstitial())
            {
                return false;
            }

            return true;
        }

        public static bool showInterstitial(string place, Action onCloseCallback)
        {
            if (!initialized)
            {
                return false;
            }

#if UNITY_IOS
            if (RuntimeInfoManager.NeedToSkipInterstitial)
            {
                return false;
            }
#endif

            if (place == null)
            {
                place = "ad_interstitial_null";
            }
            else if (place.Equals(""))
            {
                place = "ad_interstitial_empty";
            }

            SayKitDebug.Log("Showing Interstitial");

            if (SayKit.isPremium)
            {
                if (onCloseCallback != null)
                {
                    onCloseCallback();
                }
                return false;
            }

            if (shouldSkipPlace(place, isRewarded: false))
            {
                return false;
            }

            if (!hasInterstitial(false))
            {
                trackPlaceEvent(place, "interstitial_not_loaded");
                return false;
            }

            trackPlaceEvent(place, "interstitial_tag");

            interstitialCallback = onCloseCallback;
            lastIntersititalPlace = place;

            hasInterstitialCachedStatus = false;


            if (_useMaxMed)
            {
                MaxSdk.ShowInterstitial(interstitialAdUnitId);
            }
           

            return true;
        }



        internal static bool hasRewarded(bool useCachedStatus = true)
        {
            if (useCachedStatus) {
                return hasRewardedCachedStatus;
            }

            if (_useMaxMed)
            {
                return MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
            }

            return false;
            
        }

        private static void fetchRewardedCall()
        {
            if (_useMaxMed)
            {
                MaxSdk.LoadRewardedAd(rewardedAdUnitId);
                if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
                {
                    hasRewardedCachedStatus = true;
                }
            }
            
        }
        
        private static void fetchRewarded()
        {
            hasRewardedCachedStatus = false;
            shouldFetchRewarded = true;

            if (_useMaxMed)
            {
                if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
                {
                    hasRewardedCachedStatus = true;
                }
            }
        }


        public static void showRewarded(string place, Action<bool> onCloseCallback)
        {
            if (!initialized)
            {
                SayKit.trackEvent("rewarded_error", "showRewarded: Not initialized.");

                if (onCloseCallback != null)
                {
                    onCloseCallback(false);
                }
                return;
            }

            if (place == null)
            {
                place = "ad_rewarded_null";
            }
            else if (place.Equals(""))
            {
                place = "ad_rewarded_empty";
            }

            if (shouldSkipPlace(place, isRewarded: true))
            {
                if (onCloseCallback != null)
                {
                    onCloseCallback(false);
                }
                return;
            }


            if (hasRewarded(false))
            {
                trackPlaceEvent(place, "rewarded_tag");

                rewardedCallback = onCloseCallback;
                lastRewardedPlace = place;
                wasRewarded = false;

                hasRewardedCachedStatus = false;

                if (_useMaxMed)
                {
                    MaxSdk.ShowRewardedAd(rewardedAdUnitId);
                }

            }
            else
            {
                SayKit.trackEvent("rewarded_error", "showRewarded: NotLoaded.");
                fetchRewarded();

                if (onCloseCallback != null)
                {
                    onCloseCallback(false);
                }
            }
        }





        private static bool shouldFetchIntersitital = false;
        private static bool shouldFetchRewarded = false;
        private static int startedAt = 0;
        private static int lastIntersititalFetchAt = 0;
        private static int lastIntersititalShowedAt = 0;
        private static int lastRewardedFetchAt = 0;
        private static int lastRewardedShowedAt = 0;
        private static string lastIntersititalPlace = "";
        private static string lastRewardedPlace = "";
        private static Action interstitialCallback = null;
        private static Action<bool> rewardedCallback = null;
        private static bool wasRewarded = false;

        private static bool hasInterstitialCachedStatus = false;
        private static bool hasRewardedCachedStatus = false;

        private static float delayValue = 0.5f;



        public static IEnumerator fetchRoutine()
        {
            int limit = 1;

            while (true)
            {
                int now = Utils.currentTimestamp;

                // Watchdogs
                if (SayKit.remoteConfig.ads_settings.watchdog_interstitial > 0)
                {
                    if (!hasInterstitial() && now - lastIntersititalFetchAt > SayKit.remoteConfig.ads_settings.watchdog_interstitial)
                    {
                        SayKit.trackEvent("interstitial_watchdog");
                        shouldFetchIntersitital = true;
                    }
                }
                if (SayKit.remoteConfig.ads_settings.watchdog_rewarded > 0)
                {
                    if (!hasRewarded() && now - lastRewardedFetchAt > SayKit.remoteConfig.ads_settings.watchdog_rewarded)
                    {
                        SayKit.trackEvent("rewarded_watchdog");
                        shouldFetchRewarded = true;
                    }
                }

                // Fetching
                if (shouldFetchIntersitital && now - lastIntersititalFetchAt >= limit && !SayKit.isPremium)
                {

                    SayKit.trackEvent("interstitial_fetch");

                    if (!hasInterstitial())
                    {
                        fetchInterstitialCall();
                    }

                    lastIntersititalFetchAt = now;
                    shouldFetchIntersitital = false;
                }

                if (SayKit.isPremium)
                {
                    if (_useMaxMed) {
                        //TODO stop interload
                    }
                    
                }

                if (shouldFetchRewarded && now - lastRewardedFetchAt >= limit)
                {

                    SayKit.trackEvent("rewarded_fetch");

                    if (!hasRewarded())
                    {
                        fetchRewardedCall();
                    }

                    lastRewardedFetchAt = now;
                    shouldFetchRewarded = false;
                }

                yield return new WaitForSecondsRealtime(delayValue);
            }
        }




//#### Callbacks

        // Banner callbacks
        private static void OnMaxBannerShown(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            if (bannerIsShowing)
            {
                SayKitBanner.Show(MaxSdkUtils.GetAdaptiveBannerHeight(320));
            }

            SayKit.trackEvent("banner_imp", adInfo?.ToJsonString());
        }

        private static void OnMaxBannerClicked(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            SayKit.trackEvent("banner_click", adInfo?.ToJsonString());
        }

        private static void OnMaxBannerRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // SayKit.trackEvent("banner_paid", adInfo?.ToJsonString());
        }



        private static void OnMaxInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            hasInterstitialCachedStatus = true;
            int now = Utils.currentTimestamp;

            SayKit.trackEvent("interstitial_loaded", now - lastIntersititalFetchAt, 0, adInfo?.ToJsonString());
            SayKitDebug.Log("AdsManager.onInterstitialLoadedEvent");

            CleanDelayValue();
        }

        private static void OnMaxInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            SayKit.trackEvent("interstitial_error", errorInfo?.ToString());
            
            fetchInterstitial();
            IncrementDelayValue();

            SayKitDebug.Log("AdsManager.onInterstitialFailedEvent");
        }

        private static void OnMaxInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            lastIntersititalShowedAt = Utils.currentTimestamp;

            SayKitDebug.Log("AdsManager.onInterstitialShownEvent");


            SayKit.trackEvent("interstitial_imp", 0, 0, adInfo?.ToJsonString());

            double revenue = adInfo?.Revenue ?? -1;
            saveNewImpression("i", lastIntersititalShowedAt, revenue);

            updateImpression(lastIntersititalPlace);
        }

        private static void OnMaxInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            SayKit.trackEvent("interstitial_click", adInfo?.ToJsonString());
        }

        private static void OnMaxInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            SayKitDebug.Log("AdsManager.OnInterstitialDismissed");

            fetchInterstitial();

            if (interstitialCallback != null)
            {
                interstitialCallback();
                interstitialCallback = null;
            }

            SayKit.trackEvent("interstitial_close", adInfo?.ToJsonString());
        }

        private static void OnMaxInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            SayKitDebug.Log("AdsManager.OnInterstitialShowFailed");

            fetchInterstitial();

            if (interstitialCallback != null)
            {
                interstitialCallback();
                interstitialCallback = null;
            }

            SayKit.trackEvent("interstitial_failed", errorInfo?.ToString(), adInfo?.ToJsonString());
        }

        private static void OnMaxInterstitialRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // SayKit.trackEvent("interstitial_paid", adInfo?.ToJsonString());
        }



        // Rewarded callbacks
        private static void OnMaxRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            hasRewardedCachedStatus = true;
            
            int now = Utils.currentTimestamp;

            SayKit.trackEvent("rewarded_loaded", now - lastRewardedFetchAt, 0, adInfo?.ToJsonString());
            SayKitDebug.Log("AdsManager.OnRewardedLoadedEvent");

            CleanDelayValue();
        }

        private static void OnMaxRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            SayKit.trackEvent("rewarded_failed", errorInfo?.ToString());

            fetchRewarded();
            SayKitDebug.Log("AdsManager.OnRewardedFailedEvent");

            IncrementDelayValue();
        }

        private static void OnMaxRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            lastRewardedShowedAt = Utils.currentTimestamp;

            SayKit.trackEvent("rewarded_imp", 0, 0, adInfo?.ToJsonString());

            double revenue = adInfo?.Revenue ?? -1;

            saveNewImpression("r", lastRewardedShowedAt, revenue);
            updateImpression(lastRewardedPlace);
        }

        private static void OnMaxRewardedAdClickedEvent(string extra, MaxSdkBase.AdInfo adInfo)
        {
            SayKit.trackEvent("rewarded_click", adInfo?.ToJsonString());
        }

        private static void OnMaxRewardedAdHiddenEvent(string extra, MaxSdkBase.AdInfo adInfo)
        {
            SayKitDebug.Log("AdsManager.OnRewardedClosedEvent");

            if (rewardedCallback != null)
            {
                rewardedCallback(wasRewarded);
                rewardedCallback = null;
            }

            fetchRewarded();

            SayKit.trackEvent("rewarded_close", adInfo?.ToJsonString());
        }

        private static void OnMaxRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            SayKit.trackEvent("rewarded_show_failed", errorInfo?.ToString(), adInfo?.ToJsonString());

            SayKitDebug.Log("AdsManager.OnRewardedShowFailed");

            if (rewardedCallback != null)
            {
                rewardedCallback(false);
                rewardedCallback = null;
            }

            fetchRewarded();
        }

        private static void OnMaxRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            wasRewarded = true;

            SayKit.trackEvent("rewarded_received", adInfo?.ToJsonString());
        }

        private static void OnMaxRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // SayKit.trackEvent("rewarded_paid", adInfo?.ToJsonString());
        }



        // ads logic
        public static bool isRewardedAvailable(string place)
        {
            if (!initialized)
            {
                return false;
            }

            if (place == null)
            {
                place = "ad_rewarded_null";
            }
            else if (place.Equals(""))
            {
                place = "ad_rewarded_empty";
            }

            if (shouldSkipPlace(place, isRewarded: true))
            {
                return false;
            }

            if (!hasRewarded())
            {
                trackPlaceEvent(place, "rewarded_not_loaded");
                return false;
            }

            return true;
        }


        private const string ADS_PLACE_STATUS_OFF = "off";
        private const string ADS_PLACE_STATUS_ALWAYS = "always";
        private const string ADS_PLACE_STATUS_SKIP = "skip";

        private static Dictionary<string, int> placeRequests = new Dictionary<string, int>();
        private static Dictionary<string, int> placeLastRequest = new Dictionary<string, int>();


        private static bool shouldSkipPlace(string place, bool isRewarded)
        {

            if (!RemoteConfigManager.initialized)
            {
                return false;
            }

            RemoteConfigAdsPlace placeConfig = SayKit.remoteConfig.findAdsPlace(place);

            if (placeConfig == null)
            {

                trackPlaceEvent(place, "ads_place_not_found");

                placeConfig = new RemoteConfigAdsPlace();

                if (isRewarded)
                {
                    placeConfig.type = "rewarded";
                    placeConfig.status = ADS_PLACE_STATUS_ALWAYS;
                }
                else
                {
                    placeConfig.type = "interstitial";
                    placeConfig.status = ADS_PLACE_STATUS_SKIP;
                }

                placeConfig.place = place;
                placeConfig.group = "default";
            }

            if (placeConfig.status == ADS_PLACE_STATUS_OFF)
            {
                trackPlaceEvent(place, "ads_off");
                return true;
            }

            if (placeConfig.status == ADS_PLACE_STATUS_ALWAYS)
            {
                return false;
            }

            if (placeConfig.status == ADS_PLACE_STATUS_SKIP)
            {

                RemoteConfigAdsGroup groupConfig = SayKit.remoteConfig.findAdsGroup(placeConfig.group);

                if (groupConfig == null)
                {
                    trackPlaceEvent(placeConfig.group, "ads_group_not_found");
                    return true;
                }


                int ts = Utils.currentTimestamp;

                // skip on game start
                if (ts - startedAt <= GetSkipAfterStart(groupConfig.skip_after_start))
                {
                    trackPlaceEvent(place, "ads_skip_start");
                    return true;
                }

                // force impression every Nth request
                if (groupConfig.force_impression_every > 0)
                {

                    int requests = 0;
                    placeRequests.TryGetValue(groupConfig.group, out requests);

                    int lastRequest = 0;
                    placeLastRequest.TryGetValue(groupConfig.group, out lastRequest);

                    // check if this request isn't a duplicate
                    if (ts - lastRequest > 1)
                    {
                        requests++;
                        placeRequests[groupConfig.group] = requests;
                        placeLastRequest[groupConfig.group] = ts;
                    }

                    if (requests % groupConfig.force_impression_every == 0)
                    {
                        trackPlaceEvent(place, "ads_force_impression");
                        return false;
                    }
                }

                // skip after last impression
                if (ts - lastIntersititalShowedAt <= GetSkipAfterInterstitial(groupConfig.skip_after_interstitial)
                    || ts - lastRewardedShowedAt <= GetSkipAfterRewarded(groupConfig.skip_after_rewarded))
                {
                    trackPlaceEvent(place, "ads_skip");
                    return true;
                }

                // skip if limit of impressions in period of time reached
                if (groupConfig.skip_period_duration > 0)
                {
                    int periodEnds = 0;
                    int periodImpressions = 0;
                    Storage.instance.adsGroupPeriodEnds.TryGetValue(groupConfig.group, out periodEnds);
                    Storage.instance.adsGroupPeriodImpressions.TryGetValue(groupConfig.group, out periodImpressions);

                    if (periodEnds >= ts && periodImpressions >= groupConfig.skip_period_limit)
                    {
                        trackPlaceEvent(place, "ads_skip_period");
                        return true;
                    }
                }

                return false;
            }

            return true;
        }




        private static IDictionary<string, int> placesLastEvent = new Dictionary<string, int>();
        private static int _lastTrackPlaceEventTimestamp = 5;

        private static void trackPlaceEvent(string place, string eventName)
        {
            if (place == null)
            {
                place = "null";
            }

            var key = place + "_" + eventName;


            int lastTimestamp = 0;
            if (placesLastEvent.TryGetValue(key, out lastTimestamp))
            {
                if (SayKitInternal.Utils.currentTimestamp - lastTimestamp >= 15)
                {
                    placesLastEvent[key] = SayKitInternal.Utils.currentTimestamp;
                    SayKit.trackEvent(eventName, place);
                }
            }
            else
            {
                placesLastEvent.Add(key, SayKitInternal.Utils.currentTimestamp);
                SayKit.trackEvent(eventName, place);
            }
        }

        private static void saveNewImpression(string type, int time, double cpm) {

            if (SayKit.remoteConfig.runtime.disable_imp_manager == 0)
            {
                ImpressionLogManager.Instance.RunSavingImpression(new ImpressionObject(type, time), (float)cpm);
            }

#if UNITY_IOS
            if (SayKit.remoteConfig.runtime.disable_cv_manager == 0)
            {
                ConversionManager.Instance.RunRequestConversionData("imp");
            }
#endif
        }

        private static void updateImpression(string place) {
            
            RemoteConfigAdsPlace placeConfig = SayKit.remoteConfig.findAdsPlace(place);
            if (placeConfig == null) {
                return;
            }

            RemoteConfigAdsGroup groupConfig = SayKit.remoteConfig.findAdsGroup(placeConfig.group);
            if (groupConfig == null) {
                return;
            }

            int ts = Utils.currentTimestamp;

            // Reset ads requests counter for force_impression_every
            placeRequests[groupConfig.group] = 0;

            int periodEnds = 0;
            int periodImpressions = 0;
            Storage.instance.adsGroupPeriodEnds.TryGetValue(groupConfig.group, out periodEnds);
            Storage.instance.adsGroupPeriodImpressions.TryGetValue(groupConfig.group, out periodImpressions);

            if (periodEnds < ts) {
                Storage.instance.adsGroupPeriodEnds[groupConfig.group] = ts + groupConfig.skip_period_duration;
                periodImpressions = 0;
            }

            periodImpressions++;
            if (periodImpressions < 1) {
                periodImpressions = 1;
            }
            Storage.instance.adsGroupPeriodImpressions[groupConfig.group] = periodImpressions;
            Storage.instance.save();
        }


        private static int m_pow = 2;
        private static float m_maxDelayMilliseconds = 10;
        private static bool isMaxDelayAchieved =false;

        private static void IncrementDelayValue()
        {

            try
            {
                if (isMaxDelayAchieved)
                {
                    delayValue = m_maxDelayMilliseconds;
                }
                else
                {
                    m_pow = m_pow << 1; // m_pow = Pow(2, m_retries - 1)

                    float delay = (float)(0.5 * (m_pow - 1) / 2);

                    if (delay >= m_maxDelayMilliseconds)
                    {
                        isMaxDelayAchieved = true;
                        delayValue = m_maxDelayMilliseconds;
                    }
                    else
                    {
                        delayValue = delay;
                    }
                }

                SayKitDebug.Log("AdsManager IncrementDelayValue " + delayValue);
            }
            catch (Exception ex)
            {
                SayKitDebug.Log("AdsManager.IncrementDelayValue error " + ex.Message);
                CleanDelayValue();
            }
        }

        private static void CleanDelayValue()
        {
            delayValue = 0.5f;
            isMaxDelayAchieved = false;
            m_pow = 1;

            SayKitDebug.Log("AdsManager CleanDelayValue " + delayValue);
        }



        private static int GetSkipAfterStart(int skip_after_start)
        {
#if UNITY_IOS
            if (RuntimeInfoManager.NeedToUseOverriddenTimeouts())
            {
                return SayKit.remoteConfig.ads_settings.skad_skip_after_start;
            }
#endif
            return skip_after_start;
        }

        private static int GetSkipAfterInterstitial(int skip_after_interstitial)
        {
#if UNITY_IOS
            if (RuntimeInfoManager.NeedToUseOverriddenTimeouts())
            {
                return SayKit.remoteConfig.ads_settings.skad_skip_after_interstitial;
            }
#endif
            return skip_after_interstitial;
        }

        private static int GetSkipAfterRewarded(int skip_after_rewarded)
        {
#if UNITY_IOS
            if (RuntimeInfoManager.NeedToUseOverriddenTimeouts())
            {
                return SayKit.remoteConfig.ads_settings.skad_skip_after_rewarded;
            }
#endif
            return skip_after_rewarded;
        }


    }
}
