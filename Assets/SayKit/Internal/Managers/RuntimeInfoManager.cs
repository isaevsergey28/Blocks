using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

namespace SayKitInternal
{

    public class RuntimeInfo
    {
        public string version = "";
        public string idfa = "";
        public string idfv = "";
        public string deviceOs = "";
        public string deviceModel = "";
        public string language = "";
    };

    public class RuntimeInfoManager
    {

        static public RuntimeInfo runtimeInfo = new RuntimeInfo();
        static public bool initialized = false;
        static public List<int> nextPopupTimeouts = new List<int>();

        static public IEnumerator initRoutine()
        {

            PlatformManager.crashlyticsInit();

            runtimeInfo.version = Application.version;
            runtimeInfo.deviceModel = SystemInfo.deviceModel;
            runtimeInfo.deviceOs = SystemInfo.operatingSystem;
            runtimeInfo.language = getLanguage();

            // Debug.Log("runtimeInfo.language = " + runtimeInfo.language);

            runtimeInfo.idfv = Storage.instance.idfv;

            if (Storage.instance.idfv == "")
            {
                runtimeInfo.idfv = SystemInfo.deviceUniqueIdentifier;
                Storage.instance.idfv = runtimeInfo.idfv;
                Storage.instance.save();
            }



#if UNITY_EDITOR || UNITY_IOS

            var idfa = PlatformManager.getIDFA();
            if (idfa?.Length <= 0)
            {
                idfa = System.Guid.Empty.ToString();
            }

            runtimeInfo.idfa = idfa;

            initialized = true;

            yield break;

#elif UNITY_ANDROID


            PlatformManager.SetUnityIDFVToSayPromo(Storage.instance.idfv);
            bool idfaInitialized = false;

#if UNITY_2020_2_OR_NEWER

            PlatformManager.InitializeIDFAAndroid();

                while (!idfaInitialized)
                {
                    yield return new WaitForSecondsRealtime(0.1f);

                    String advertisingId = PlatformManager.getIDFA();
                    Debug.Log("IDFA: " + advertisingId);

                    if (advertisingId.Length > 0)
                    {
                        runtimeInfo.idfa = advertisingId;
                        idfaInitialized = true;
                    }
                }
#else
            if (Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) =>
            {
                if (advertisingId?.Length <= 0)
                {
                    advertisingId = System.Guid.Empty.ToString();
                }

                runtimeInfo.idfa = advertisingId;
                idfaInitialized = true;
            }))

            
            while (!idfaInitialized)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }
#endif

            initialized = true;
#endif

        }


        static string getLanguage()
        {

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Chinese:
                    return "zh";
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.ChineseTraditional:
                    return "zh";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Arabic:
                    return "ar";
                default:
                    var nativeSystemLang = PlatformManager.getNativeSystemLanguage();
                    if (nativeSystemLang == "hi")
                    {
                        return nativeSystemLang;
                    }

                    return "en";
            }
        }



#if UNITY_IOS
        public static bool IDFAInitialized = false;
#else
        public static bool IDFAInitialized = true;
#endif

        private static bool isInitializeIDFACalled = false;
        public static void InitializeIDFA()
        {
#if UNITY_IOS

            SayKitDebug.Log("skad_imitation: " + SayKit.remoteConfig.ads_settings.skad_imitation);
            if (!isInitializeIDFACalled)
            {
                isInitializeIDFACalled = true;

                if (SayKit.remoteConfig.ads_settings.skad_imitation == 1)
                {
                    ConfiguratePopupTimeouts();
                    StartIDFACheck();
                }
                else
                {
                    _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Authorized;
                    IDFAInitialized = true;
                }
            }
#endif
        }

#if UNITY_IOS

        public static readonly string SKAD_POPUP_FIRST_SHOWED = "SKAD_POPUP_FIRST_SHOWED";
        public static readonly string SKAD_AUTH_STATUS_SYSTEM_DENIED = "SKAD_AUTH_STATUS_SYSTEM_DENIED";
        public static readonly string SKAD_POPUP_SHOWN_COUNT = "SKAD_POPUP_SHOWN_COUNT";

        public static readonly string _IDFAAuthorized = @"Authorized";
        public static readonly string _IDFADenied = @"Denied";


        private static ATTrackingAuthorizationStatus _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.None;
        private static bool _checkIDFAStatusOnLevelChange = false;
        private static int _lastIDFAPopupShowTimestamp = 0;
        private static int _lastIDFAPopupShowLevel = 0;

        private static bool _isPopupShowed = false;
        public static bool NeedToSkipInterstitial
        {
            get
            {
                if (SayKit.remoteConfig.ads_settings.skad_popup_layout_unity == 61)
                {
                    return false;
                }
                return _isPopupShowed;
            }
        }


        private static string _currentPopup = "";

        private static void ShowSystemIDFAPopup()
        {
            if (!_isPopupShowed)
            {
                _isPopupShowed = true;

                SayKitDebug.Log("ShowSystemIDFAPopup");
                PlatformManager.sayKitShowSystemIDFAPopup();
            }
        }

        private static void ShowNativeIDFAPopup()
        {
            if (!_isPopupShowed)
            {
                _isPopupShowed = true;

                SayKitDebug.Log("ShowNativeIDFAPopup");

                PlatformManager.sayKitShowNativeIDFAPopup(
                    title: SayKit.getLocalizedString("skad_popup_title"),
                    description: SayKit.getLocalizedString("skad_popup_text"),
                    okBtnString: SayKit.getLocalizedString("skad_popup_ok"),
                    cancelBtnString: SayKit.getLocalizedString("skad_popup_cancel")
                    );

                _currentPopup = "native";
                trackPopupShownEvent("native");
            }
        }

        private static void ShowUnityIDFAPopup()
        {
            if (!_isPopupShowed)
            {
                _isPopupShowed = true;
                SayKitDebug.Log("ShowUnityIDFAPopup: " + SayKit.remoteConfig.ads_settings.skad_popup_layout_unity);

                _currentPopup = "unity";
                trackPopupShownEvent("unity");

                var instance = SayKitIDFAUI.getInstance("IDFA_" + SayKit.remoteConfig.ads_settings.skad_popup_layout_unity);
                if (instance != null)
                {
                    SayKit.hideBanner();
                    instance.ShowPopup();
                }
            }
        }


        private static void GetIDFA()
        {

            var idfa = PlatformManager.getIDFA();
            if (idfa?.Length <= 0)
            {
                idfa = System.Guid.Empty.ToString();
            }

            runtimeInfo.idfa = idfa;
        }

        private static void StartIDFACheck()
        {
            UpdateCurrentIDFAStatus();

            switch (_trackingAuthorizationStatus)
            {
                case ATTrackingAuthorizationStatus.Authorized:
                    SayKitDebug.Log("ATTrackingAuthorizationStatus: Authorized");

                    var idfa = PlatformManager.getIDFA();
                    if (idfa?.Length <= 0)
                    {
                        idfa = System.Guid.Empty.ToString();
                    }

                    runtimeInfo.idfa = idfa;
                    IDFAInitialized = true;

                    break;

                case ATTrackingAuthorizationStatus.Denied:

                    var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;
                    if (skad_popup_first_showed)
                    {
                        CheckIDFAPopUpStatus();
                    }
                    else
                    {
                        IDFAInitialized = true;
                    }

                    break;

                case ATTrackingAuthorizationStatus.NotDetermined:

                    if (SayKit.remoteConfig.ads_settings.skad_nd_first_popup_only == 1)
                    {
                        PlayerPrefs.SetInt(SKAD_POPUP_FIRST_SHOWED, 0);
                    }

                    CheckIDFAPopUpStatus();

                    break;

                case ATTrackingAuthorizationStatus.Restricted:

                    IDFAInitialized = true;

                    break;

                case ATTrackingAuthorizationStatus.SystemDenied:

                    if (SayKit.remoteConfig.ads_settings.skad_dnt_behaviour == 1)
                    {
                        CheckIDFAPopUpStatus();
                        // turn on
                        SayKitDebug.Log("ATTrackingAuthorizationStatus: SystemDenied with checking.");
                    }
                    else
                    {
                        IDFAInitialized = true;
                        SayKitDebug.Log("ATTrackingAuthorizationStatus: SystemDenied without checking.");
                    }

                    break;
            }
        }



        private static void CheckIDFAPopUpStatus()
        {
            SayKitDebug.Log("CheckIDFAPopUpStatus");

            var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;

            if (skad_popup_first_showed)
            {
                //second start

                var skadPopup = SayKit.remoteConfig.ads_settings.skad_popup_next;

                if (skadPopup == "timer")
                {
                    IDFAInitialized = true;
                    ShowIDFAPopupWithDelay(SayKit.remoteConfig.ads_settings.skad_popup_next_count);
                }
                else if (skadPopup == "start")
                {
                    SayKitDebug.Log("CheckIDFAPopUpStatus 2-time");
                    ShowIDFAPopup();
                }
                else if (skadPopup != "never")
                {
                    IDFAInitialized = true;
                    _checkIDFAStatusOnLevelChange = true;
                }
                else
                {
                    IDFAInitialized = true;
                }
            }
            else
            {
                //first start
                
                var skadPopup = SayKit.remoteConfig.ads_settings.skad_popup_first;

                if (skadPopup == "start")
                {
                    SayKitDebug.Log("CheckIDFAPopUpStatus 1-time");
                    ShowIDFAPopup();
                }
                else if (skadPopup == "timer")
                {
                    IDFAInitialized = true;
                    ShowIDFAPopupWithDelay(SayKit.remoteConfig.ads_settings.skad_popup_first_count);
                }
                else
                {
                    IDFAInitialized = true;
                    _checkIDFAStatusOnLevelChange = true;
                }
            }
        }

        private static void UpdateCurrentIDFAStatus()
        {
            var _prevTrackingStatus = _trackingAuthorizationStatus;
            var idfaStatus = PlatformManager.sayKitGetCurrentIDFAStatus();

            //SayKitDebug.Log("UpdateCurrentIDFAStatus: " + idfaStatus);


            if (idfaStatus == "ATTrackingManagerAuthorizationStatusRestricted")
            { _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Restricted; }
            else if (idfaStatus == "ATTrackingManagerAuthorizationStatusAuthorized")
            { _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Authorized; }
            else if (idfaStatus == "ATTrackingManagerAuthorizationStatusDenied")
            {
                var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;
                if (!skad_popup_first_showed)
                {
                    _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.SystemDenied;
                }
                else
                {
                    var skad_auth_status_system_denied = PlayerPrefs.GetInt(SKAD_AUTH_STATUS_SYSTEM_DENIED) == 1;
                    if (skad_auth_status_system_denied)
                    {
                        _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.SystemDenied;
                    }
                    else
                    {
                        _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Denied;
                    }
                }
            }
            else if (idfaStatus == "ATTrackingManagerAuthorizationStatusNotDetermined")
            {
                _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.NotDetermined;

                PlayerPrefs.SetInt(SKAD_AUTH_STATUS_SYSTEM_DENIED, 0);
            }


            if (_prevTrackingStatus != _trackingAuthorizationStatus)
            {
                AnalyticsEvent.trackEvent("skad_status", _trackingAuthorizationStatus.ToString());
            }
        }



        public static void OnIDFAStatusResult(string result)
        {
            _isPopupShowed = false;
            _lastIDFAPopupShowTimestamp = Utils.currentTimestamp;


            var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;
            if (!skad_popup_first_showed)
            {
                //first
                var isModal = SayKit.remoteConfig.ads_settings.skad_popup_behaviour == 1;
                if (isModal && result == _IDFADenied)
                {
                    trackPopupClosedEvent(0);
                    ShowIDFAPopup(isModal);

                    return;
                }
            }
            else
            {
                //second
                var isSecondModal = SayKit.remoteConfig.ads_settings.skad_popup_behaviour_second == 1;
                if (isSecondModal && result == _IDFADenied)
                {
                    trackPopupClosedEvent(0);
                    ShowIDFAPopup(isSecondModal);

                    return;
                }
            }


            IDFAInitialized = true;

            var skadPopup = SayKit.remoteConfig.ads_settings.skad_popup_next;
            if (skadPopup == "level")
            {
                _checkIDFAStatusOnLevelChange = true;
            }


            if (result == _IDFAAuthorized)
            {
                SayKitDebug.Log("IDFAStatus Authorized");
                trackPopupClosedEvent(1);

                AnalyticsEvent.trackEvent("skad_status_changed", 0, 0, _trackingAuthorizationStatus.ToString(), 0, 0, "Authorized");
                _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Authorized;
                _checkIDFAStatusOnLevelChange = false;

                GetIDFA();

                AnalyticsEvent.trackEvent("skad_status", _trackingAuthorizationStatus.ToString());
            }
            else
            {
                SayKitDebug.Log("IDFAStatus Denied");
                trackPopupClosedEvent(0);

                var skad_auth_status_system_denied = PlayerPrefs.GetInt(SKAD_AUTH_STATUS_SYSTEM_DENIED) == 1;
                if (_trackingAuthorizationStatus == ATTrackingAuthorizationStatus.SystemDenied
                    || skad_auth_status_system_denied)
                {
                    PlayerPrefs.SetInt(SKAD_AUTH_STATUS_SYSTEM_DENIED, 1);
                }
                else
                {
                    AnalyticsEvent.trackEvent("skad_status_changed", 0, 0, _trackingAuthorizationStatus.ToString(), 0, 0, "Denied");
                    _trackingAuthorizationStatus = ATTrackingAuthorizationStatus.Denied;
                }


                if (!skad_popup_first_showed)
                {
                    PlayerPrefs.SetInt(SKAD_POPUP_FIRST_SHOWED, 1);
                }

                AnalyticsEvent.trackEvent("skad_status", _trackingAuthorizationStatus.ToString());

                CheckIDFAPopUpStatus();
            }

        }


        public static void OnUnityPopupResult(bool result)
        {
            _isPopupShowed = false;

            if (result)
            {
                var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;

                if (skad_popup_first_showed)
                {
                    _isPopupShowed = true;

                    PlatformManager.sayKitShowSystemIDFAPopup();
                }
                else
                {
                    ShowSystemIDFAPopup();
                }
            }
            else
            {
                OnIDFAStatusResult(_IDFADenied);
            }
        }

        public static void OnLevelCompleted(int level)
        {
            //Debug.Log("OnLevelCompleted: " + _checkIDFAStatusOnLevelChange);


            if (_checkIDFAStatusOnLevelChange)
            {
                if (SayKit.remoteConfig.ads_settings.skad_imitation == 1)
                {
                    if (_trackingAuthorizationStatus != ATTrackingAuthorizationStatus.Authorized)
                    {

                        var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;
                        if (skad_popup_first_showed)
                        {
                            // second + show

                            var skadPopup = SayKit.remoteConfig.ads_settings.skad_popup_next;

                            SayKitDebug.Log("OnLevelCompleted: " + skadPopup);


                            if (skadPopup == "level")
                            {
                                if (level - _lastIDFAPopupShowLevel >= SayKit.remoteConfig.ads_settings.skad_popup_next_count)
                                {
                                    ShowIDFAPopup();

                                    _lastIDFAPopupShowLevel = level;
                                }
                            }
                            else if (skadPopup == "timeout")
                            {
                                if (Utils.currentTimestamp - _lastIDFAPopupShowTimestamp > SayKit.remoteConfig.ads_settings.skad_popup_next_count)
                                {
                                    ShowIDFAPopup();
                                }
                            }
                        }
                        else
                        {
                            // first show

                            var skadPopup = SayKit.remoteConfig.ads_settings.skad_popup_first;

                            if (skadPopup == "level")
                            {
                                if (level - _lastIDFAPopupShowLevel >= SayKit.remoteConfig.ads_settings.skad_popup_first_count)
                                {
                                    ShowIDFAPopup();

                                    _lastIDFAPopupShowLevel = level;
                                }
                            }
                        }
                    }


                }
            }

        }


        private static void ShowIDFAPopupWithDelay(int delay)
        {

            if (SayKit.remoteConfig.ads_settings.skad_popup_next_max > 0)
            {
                var count = PlayerPrefs.GetInt(SKAD_POPUP_SHOWN_COUNT) - 1; // Doesn't count the first one
                if (count >= SayKit.remoteConfig.ads_settings.skad_popup_next_max)
                {
                    return;
                }
            }

            if (nextPopupTimeouts?.Count > 0)
            {
                var count = PlayerPrefs.GetInt(SKAD_POPUP_SHOWN_COUNT) - 1; // Doesn't count the first one
                if (count >= 0
                    && nextPopupTimeouts.Count - 1 >= count)
                {
                    delay = nextPopupTimeouts[count];
                }
                SayKitDebug.Log("ShowIDFAPopupWithDelay: Count" + count
                    + ", Length: " + (nextPopupTimeouts.Count - 1));
            }


            SayKitDebug.Log("ShowIDFAPopupWithDelay: " + delay);

            var task = Task.Factory.StartNew(() => Thread.Sleep(delay * 1000))
                         .ContinueWith((t) =>
                         {
                             SayKitDebug.Log("ShowIDFAPopupWithDelay: show");

                             ShowIDFAPopup();
                         }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private static void ShowIDFAPopup(bool isModal = false)
        {
            SayKitDebug.Log("ShowIDFAPopup");
            UpdateCurrentIDFAStatus();

            if (_trackingAuthorizationStatus != ATTrackingAuthorizationStatus.Authorized)
            {
                if (!isModal && SayKit.remoteConfig.ads_settings.skad_popup_next_max > 0)
                {
                    var count = PlayerPrefs.GetInt(SKAD_POPUP_SHOWN_COUNT) - 1; // Doesn't count the first one
                    if (count >= SayKit.remoteConfig.ads_settings.skad_popup_next_max)
                    {
                        SayKitDebug.Log("ShowIDFAPopup: Max count of popups = " + count);
                        return;
                    }
                }

                if (SayKit.remoteConfig.ads_settings.skad_popup_layout == 1)
                {
                    ShowNativeIDFAPopup();
                }
                else if (SayKit.remoteConfig.ads_settings.skad_popup_layout == 2)
                {
                    ShowUnityIDFAPopup();
                }
                else if (SayKit.remoteConfig.ads_settings.skad_popup_layout == 3)
                {
                    var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;

                    if (!skad_popup_first_showed)
                    {
                        trackPopupShownEvent("system");
                        ShowSystemIDFAPopup();
                    }
                    else { ShowNativeIDFAPopup(); }
                }
                else if (SayKit.remoteConfig.ads_settings.skad_popup_layout == 4)
                {
                    var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;

                    if (!skad_popup_first_showed)
                    {
                        trackPopupShownEvent("system");
                        ShowSystemIDFAPopup();
                    }
                    else { ShowUnityIDFAPopup(); }
                }
            }

        }



        public static bool NeedToUseOverriddenTimeouts()
        {
            if (_trackingAuthorizationStatus != ATTrackingAuthorizationStatus.Authorized)
            {
                if (SayKit.remoteConfig.ads_settings.skad_imitation == 1 &&
                        SayKit.remoteConfig.ads_settings.skad_optout_policy == 1)
                {
                    return true;
                }
            }

            return false;
        }


        private static void trackPopupShownEvent(string type)
        {
            var count = PlayerPrefs.GetInt(SKAD_POPUP_SHOWN_COUNT);
            count += 1;

            AnalyticsEvent.trackEvent("skad_popup_shown", count, 0, type);
            PlayerPrefs.SetInt(SKAD_POPUP_SHOWN_COUNT, count);
        }

        private static void trackPopupClosedEvent(int result)
        {
            var count = PlayerPrefs.GetInt(SKAD_POPUP_SHOWN_COUNT);

            AnalyticsEvent.trackEvent("skad_popup_closed", count, result, _currentPopup);
            _currentPopup = "";
        }
#endif

        public static bool DeepLinkToSettingsCalled = false;
        public static void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SayKitDebug.Log("OnApplicationPause: pause");
            }
            else
            {
                SayKitDebug.Log("OnApplicationPause: resume");

#if UNITY_IOS

                if (SayKit.remoteConfig.ads_settings.skad_imitation == 1)
                {
                    if (_isPopupShowed)
                    {
                        if (DeepLinkToSettingsCalled)
                        {
                            IDFAInitialized = true;


                            DeepLinkToSettingsCalled = false;
                            _isPopupShowed = false;

                            UpdateCurrentIDFAStatus();


                            if (_trackingAuthorizationStatus != ATTrackingAuthorizationStatus.Authorized)
                            {
                                trackPopupClosedEvent(3);

                                ShowPopupWithModalCheck();
                            }
                            else
                            {
                                AnalyticsEvent.trackEvent("skad_status_changed", 0, 0, _trackingAuthorizationStatus.ToString(), 0, 0, "Authorized");
                                GetIDFA();
                                trackPopupClosedEvent(2);
                            }
                        }
                        else
                        {
                            //Debug.Log("OnApplicationPause: resume 2");
                        }
                    }
                    else
                    {
                        IDFAInitialized = true;

                        var prevTrackingAuthorizationStatus = _trackingAuthorizationStatus;
                        UpdateCurrentIDFAStatus();

                        if (_trackingAuthorizationStatus != ATTrackingAuthorizationStatus.Authorized)
                        {
                            if (prevTrackingAuthorizationStatus == ATTrackingAuthorizationStatus.Authorized)
                            {
                                ShowPopupWithModalCheck();
                            }
                        }
                    }

                }
#endif

            }
        }

#if UNITY_IOS
        private static void ShowPopupWithModalCheck()
        {
            var skad_popup_first_showed = PlayerPrefs.GetInt(SKAD_POPUP_FIRST_SHOWED) == 1;
            if (!skad_popup_first_showed)
            {
                var isFirstModal = SayKit.remoteConfig.ads_settings.skad_popup_behaviour == 1;
                if (isFirstModal)
                {
                    ShowIDFAPopup(isFirstModal);
                    return;
                }
            }

            PlayerPrefs.SetInt(SKAD_POPUP_FIRST_SHOWED, 1);

            var isSecondModal = SayKit.remoteConfig.ads_settings.skad_popup_behaviour_second == 1;
            if (isSecondModal)
            {
                ShowIDFAPopup(isSecondModal);
                return;
            }

            CheckIDFAPopUpStatus();
        }
#endif
        private static void ConfiguratePopupTimeouts()
        {
            nextPopupTimeouts = new List<int>();

            try
            {
                string[] skad_popup_next_timeouts = SayKit.remoteConfig.ads_settings.skad_popup_next_timeouts.Split(';');
                for (int i = 0; i < skad_popup_next_timeouts.Length; i++)
                {
                    if (Int32.TryParse(skad_popup_next_timeouts[i], out int numValue))
                    {
                        nextPopupTimeouts.Add(numValue);
                    }
                }
            }
            catch (Exception ex)
            {
                SayKitDebug.Log("RuntimeInfoManager: " + ex.Message);
            }
        }

    }
}

    