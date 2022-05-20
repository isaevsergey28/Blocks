using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace SayKitInternal
{

    public class LocalizedMessage
    {
        public List<string> messages = new List<string>();
        public bool defaultValue;
    }


    [Serializable]
    public class RemoteConfigAdsPlace
    {
        public string place;
        public string group;
        public string status;
        public string type;
    };

    [Serializable]
    public class RemoteConfigAdsGroup
    {
        public string group;
        public int skip_after_start = 0;
        public int skip_after_interstitial = 0;
        public int skip_after_rewarded = 0;
        public int skip_period_duration = 0;
        public int skip_period_limit = 0;
        public int force_impression_every = 0;
    };

    [Serializable]
    public class RemoteConfigAdsSettings
    {
        public string banner_placement;
        public string interstitial_placement;
        public string rewarded_placement;
        public string bidding_interstitial_placement = "";
        public string bidding_rewarded_placement = "";

        public int watchdog_interstitial = 0;
        public int watchdog_rewarded = 0;
        public int banner_disabled = 0;

        public string banner_bg_color = "";
        public int banner_bg_padding = 0;

        public string ping_events = "";

        public int bidding_enabled = 0;
        public int bidding_fb_enabled = 0;
        public string bidding_fb_app = "";
        public string bidding_fb_interstitial = "";
        public string bidding_fb_rewarded = "";
        public int bidding_al_enabled = 0;
        public int bidding_sp_enabled = 0;

        public int track_waterfall = 0;

        public int saymed_enabled = 0;
        public string saymed_banner_id = "";
        public string saymed_interstitial_id = "";
        public string saymed_rewarded_id = "";

        public int maxsdk_enabled = 0;
        public string maxsdk_key = "";
        public string maxsdk_banner_id = "";
        public string maxsdk_interstitial_id = "";
        public string maxsdk_rewarded_id = "";



        /// <summary>
        /// SKAdNetwork control.
        /// 1 - turn on,
        /// other - turn off.
        /// </summary>
        public int skad_imitation = 0;

        /// <summary>
        /// DNT control.
        /// 0 - Do not force user.
        /// 1 - Force user to turn on IDFA.
        /// </summary>
        public int skad_dnt_behaviour = 0;


        /// <summary>
        /// IDFA popup behavior.
        /// 0 - user can close popup until skad_popup_next.
        /// 1 - user cannot close the popup.
        /// </summary>
        public int skad_popup_behaviour = 0;

        /// <summary>
        /// IDFA popup behavior.
        /// 0 - user can close popup until skad_popup_next.
        /// 1 - user cannot close the popup.
        /// </summary>
        public int skad_popup_behaviour_second = 0;

        /// <summary>
        /// Custom popup type.
        /// 0 - Nothing
        /// 1 - Native
        /// 2 - Unity
        /// 3 - Firstly, system popup, then native.
        /// 4 - Firstly, system popup, then unity.
        /// </summary>
        public int skad_popup_layout = 3;

        /// <summary>
        /// Unity popup type.
        /// 12
        /// 21
        /// 31
        /// </summary>
        public int skad_popup_layout_unity = 0;

        /// <summary>
        /// First popup view.
        /// start - on start of app
        /// level - N level complete.
        /// timer - N sec after start of game.
        /// </summary>
        public string skad_popup_first = "start";

        /// <summary>
        /// First popup count.
        /// if "level" - N level complete.
        /// if "timer" - N sec after start of game.
        /// </summary>
        public int skad_popup_first_count = 0;

        /// <summary>
        /// Second and next popup behavior.
        /// never
        /// level - every N level complite
        /// timer - after N sec after closing previous popup.
        /// timeout - after N sec after closing previous popup. On level complete.
        /// </summary>
        public string skad_popup_next = "never";

        /// <summary>
        /// Second and next popup count.
        /// if "level" - N level complite
        /// if "timer" - N sec after closing previous popup.
        /// if "timeout" - N sec after closing previous popup. On level complete.
        /// </summary>
        public int skad_popup_next_count = 0;

        /// <summary>
        /// Max count of popups after the first one.
        /// </summary>
        public int skad_popup_next_max = 0;

        /// <summary>
        /// Array of timeouts. Timeout n for popup n.
        /// </summary>
        public string skad_popup_next_timeouts = "";

        /// <summary>
        /// Behavior without IDFA.
        /// 1 - override ads timeout.
        /// 0 and other - nothing
        /// </summary>
        public int skad_optout_policy = 0;

        /// <summary>
        /// Show first popup only for NotDetermined status.
        /// </summary>
        public int skad_nd_first_popup_only = 0;

        // Ads timeouts. 
        public int skad_skip_after_start = 10;
        public int skad_skip_after_interstitial = 10;
        public int skad_skip_after_rewarded = 15;
        // Ads timeouts. 

    };

    [Serializable]
    public class RemoteConfigRuntime
    {
        public int timestamp = 0;
        public int segment = 0;

        public int debug = 0;
        public int debug_mediation = 0;

        public int status = 1;
        public int gdpr_applicable = 0;

        public int disable_imp_manager = 0;
        public int disable_cv_manager = 0;
    };

    [System.Serializable]
    public class RemoteLocalizedMessages
    {
        public string code;
        public string text_en;
        public string text_ru;
        public string text_de;
        public string text_fr;
        public string text_es;
        public string text_it;
        public string text_pt;
        public string text_ja;
        public string text_zh;
        public string text_ko;
        public string text_ar;
        public string text_hi;
    };

    [Serializable]
    public class RemoteConfig
    {
        public RemoteConfigAdsPlace[] ads_places;
        public RemoteConfigAdsGroup[] ads_groups;
        public RemoteConfigAdsSettings ads_settings;
        public RemoteConfigRuntime runtime = new RemoteConfigRuntime();
        public SayKitGameConfig game_settings = new SayKitGameConfig();
        public RemoteLocalizedMessages[] game_messages;

        public RemoteConfigAdsPlace findAdsPlace(string place)
        {
            return Array.Find(ads_places, item => item.place == place);
        }
        public RemoteConfigAdsGroup findAdsGroup(string group)
        {
            return Array.Find(ads_groups, item => item.group == group);
        }

        private Dictionary<string, LocalizedMessage> localizedMessages = new Dictionary<string, LocalizedMessage>();

        public void init()
        {
            
#if UNITY_EDITOR
            var systemLanguage = SayKit.config.overrideSystemLanguage;
#else
            var systemLanguage = (SayKitLanguage)Application.systemLanguage;
#endif

            if (systemLanguage == SayKitLanguage.Unknown)
            {
                string nativeSystemLang = PlatformManager.getNativeSystemLanguage();

                if (nativeSystemLang == "hi")
                {
                    systemLanguage = SayKitLanguage.Hindi;
                }
            }

            setupLocalization(systemLanguage);
        }

        public bool hasLocalizedMessage(string code)
        {
            return localizedMessages.ContainsKey(code);
        }

        public (string, bool) getLocalizedMessage(string code, string val1 = null, string val2 = null, string val3 = null, string val4 = null, string val5 = null)
        {
            if (localizedMessages.ContainsKey(code))
            {
                var texts = localizedMessages[code].messages;
                var text = texts[UnityEngine.Random.Range(0, texts.Count)];

                if (val1 != null)
                {
                    text = text.Replace("{1}", val1);
                }
                if (val2 != null)
                {
                    text = text.Replace("{2}", val2);
                }
                if (val3 != null)
                {
                    text = text.Replace("{3}", val3);
                }
                if (val4 != null)
                {
                    text = text.Replace("{4}", val4);
                }
                if (val5 != null)
                {
                    text = text.Replace("{5}", val5);
                }

                return (text, localizedMessages[code].defaultValue);
            }

            return (code, false);
        }


        public void setupLocalization(SayKitLanguage systemLanguage)
        {
            PlatformManager.CurrentLanguage = systemLanguage;
            localizedMessages = new Dictionary<string, LocalizedMessage>();


            for (int i = 0; i < game_messages.Length; i++)
            {
                if (!localizedMessages.ContainsKey(game_messages[i].code))
                {
                    localizedMessages[game_messages[i].code] = new LocalizedMessage();
                }

                if (systemLanguage == SayKitLanguage.Russian && game_messages[i].text_ru != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_ru);
                }
                else if (systemLanguage == SayKitLanguage.German && game_messages[i].text_de != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_de);
                }
                else if (systemLanguage == SayKitLanguage.French && game_messages[i].text_fr != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_fr);
                }
                else if (systemLanguage == SayKitLanguage.Spanish && game_messages[i].text_es != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_es);
                }
                else if (systemLanguage == SayKitLanguage.Italian && game_messages[i].text_it != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_it);
                }
                else if (systemLanguage == SayKitLanguage.Portuguese && game_messages[i].text_pt != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_pt);
                }
                else if (systemLanguage == SayKitLanguage.Japanese && game_messages[i].text_ja != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_ja);
                }
                else if (systemLanguage == SayKitLanguage.Chinese && game_messages[i].text_zh != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_zh);
                }
                else if (systemLanguage == SayKitLanguage.ChineseSimplified && game_messages[i].text_zh != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_zh);
                }
                else if (systemLanguage == SayKitLanguage.ChineseTraditional && game_messages[i].text_zh != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_zh);
                }
                else if (systemLanguage == SayKitLanguage.Korean && game_messages[i].text_ko != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_ko);
                }
                else if (systemLanguage == SayKitLanguage.Arabic && game_messages[i].text_ar != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_ar);
                }
                else if (systemLanguage == SayKitLanguage.Hindi && game_messages[i].text_hi != "")
                {
                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_hi);
                }
                else
                {
                    if (systemLanguage != SayKitLanguage.English)
                    {
                        localizedMessages[game_messages[i].code].defaultValue = true;
                    }

                    localizedMessages[game_messages[i].code].messages.Add(game_messages[i].text_en);
                }
            }


        }


    };

    class RemoteConfigManager
    {

        static public bool initialized = false;
        static public bool shouldInitLocal = true;
        static public RemoteConfig config = new RemoteConfig();

        static public void initLocal(string appKey, string version)
        {
            shouldInitLocal = false;

            string key = "saykit_" + appKey + "_" + version;
            string cachePath = Application.persistentDataPath + "/" + key;
            string data = "";


            try
            {
                if (System.IO.File.Exists(cachePath))
                {
                    data = System.IO.File.ReadAllText(cachePath);

                    SayKit.trackEvent("config_loaded", "cached");
                    SayKitDebug.Log("Success from cached.");
                }

                // try to read from embedded config
                if (data.Length == 0 || data[0] != '{')
                {
                    TextAsset targetFile = Resources.Load<TextAsset>(key);
                    if (targetFile != null)
                    {
                        SayKit.trackEvent("config_loaded", "embedded");
                        SayKitDebug.Log("Success from embedded.");
                        data = targetFile.text;
                    }
                }


                if (data.Length > 0 && data[0] == '{')
                {
                    config = JsonUtility.FromJson<RemoteConfig>(data);
                    config.init();

                    PlayerPrefs.SetInt("sayKitDebugFlag", config.runtime.debug);
                    SayKitDebug.InitDebugLogs();
                }
                else
                {
                    SayKit.trackEventWithoutInit("rcm_local_exc", "No data: " + data);
                }
            }
            catch (Exception exc)
            {
                shouldInitLocal = true;

                SayKit.trackEventWithoutInit("rcm_local_exc", exc.Message + " Data size: " + data?.Length);

                Debug.LogError("RemoteConfigManager initLocal data size: " + data?.Length + " json: " + data);
                Debug.LogError("RemoteConfigManager initLocal exception: " + exc.Message);
            }
        }


        static public IEnumerator initRoutine()
        {

            string key = "saykit_" + SayKit.config.appKey + "_" + SayKit.runtimeInfo.version;
            string cachePath = Application.persistentDataPath + "/" + key;
            string url = "https://app.saygames.io/config/" + SayKit.config.appKey
                + "?version=" + SayKit.runtimeInfo.version
                + "&device_id=" + SayKit.runtimeInfo.idfv
                + "&idfa=" + SayKit.runtimeInfo.idfa
                + "&saykit=" + SayKit.GetVersion
                + "&lng=" + PlatformManager.getNativeSystemLanguage()
                + "&_=" + UnityEngine.Random.Range(100000000, 900000000)
                + "&device_name=" + Uri.EscapeDataString(SayKit.runtimeInfo.deviceModel);

            SayKitDebug.Log("Config key is " + key);

            // Step 1. Before loading from web, load saved or embedded
            if (shouldInitLocal)
            {
                initLocal(SayKit.config.appKey, SayKit.runtimeInfo.version);
            }

            // Step 2. Try to load from web
            SayKitDebug.Log("Getting config from " + url);


            string backupListJson = "";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 5;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    SayKit.trackEventWithoutInit("rcm_remote_exc", "NetworkError: " + webRequest.error);
                    Debug.Log("SayKit: DownloadAvailableDatabaseList NetworkError: " + webRequest.error);
                }
                else
                {
                    backupListJson = webRequest.downloadHandler?.text;
                }
            }
            SayKitDebug.Log("SayKit: Check gzip data:" + backupListJson);

            try
            {
                if (backupListJson?.Length > 0 && backupListJson[0] == '{')
                {
                    string data = backupListJson;

                    config = JsonUtility.FromJson<RemoteConfig>(data);
                    config.init();

                    SayKit.trackEvent("config_loaded", "network");

                    SayKitDebug.Log("Saving config to " + cachePath);
                    System.IO.File.WriteAllText(cachePath, data);
                }
                else
                {
                    SayKit.trackEventWithoutInit("rcm_remote_exc", "Wrong data: " + backupListJson);
                }
            }
            catch (Exception exc)
            {
                SayKit.trackEventWithoutInit("rcm_remote_exc", exc.Message + " Data size: " + backupListJson?.Length);

                Debug.LogError("RemoteConfigManager initRoutine data size: " + backupListJson?.Length + " json: " + backupListJson);
                Debug.LogError("RemoteConfigManager initRoutine exception: " + exc.Message);
            }

            initialized = true;
        }




        private static readonly string _reload_url = "https://live.saygames.io/live/migrate";
        private static readonly int _requestTimeout = 5;

        private static String GetMigrateUrl()
        {
            string appVersion = SayKit.remoteConfig.runtime.segment == 0 ? SayKit.runtimeInfo.version : SayKit.runtimeInfo.version + "." + SayKit.remoteConfig.runtime.segment;

            string url = _reload_url + "?idfa=" + SayKit.runtimeInfo.idfa
                + "&idfv=" + SayKit.runtimeInfo.idfv
                + "&appKey=" + SayKit.config.appKey
                + "&version=" + SayKit.runtimeInfo.version
                + "&ts=" + Utils.currentTimestamp
                + "&installTs=" + ImpressionLogManager.Instance.ImpressionData.FirstStartTimestamp
                + "&saykit=" + SayKit.GetVersion
                + "&config_version=" + appVersion;

            return url;
        }

        public static IEnumerator RequestConfigMigration(string sourceVersion)
        {
            string url = GetMigrateUrl();
            string jsonData = "{\"source\": \"" + sourceVersion + "\"}";

            SayKitDebug.Log("SayKit: ReloadConfigManually " + url + " data: " + jsonData);

            
            ConfigMigrationResponseData configMigrationResponseData = new ConfigMigrationResponseData();

            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                webRequest.SetRequestHeader("Accept", "application/json");

                webRequest.timeout = _requestTimeout;

                yield return webRequest.SendWebRequest();


                if (webRequest.isNetworkError)
                {
                    SayKitDebug.Log("SayKit: ReloadConfigManually: " + webRequest.error);
                }
                else
                {
                    try
                    {
                        SayKitDebug.Log("SayKit: ReloadConfigManually: " + webRequest.downloadHandler.text);
                        configMigrationResponseData = JsonUtility.FromJson<ConfigMigrationResponseData>(webRequest.downloadHandler.text);
                    }
                    catch (Exception exp)
                    {
                        SayKitDebug.Log("SayKit: ReloadConfigManually error: " + exp.Message);
                    }


                    if (configMigrationResponseData.reloadConfig == 1)
                    {
                        SayKitDebug.Log("SayKit: ReloadConfigManually: Reload Config");

                        yield return RemoteConfigManager.initRoutine();
                        SayKit.initializeRemoteConfig();


                        SayKitDebug.Log("SayKit: ReloadConfigManually: update segment " + SayKit.remoteConfig.runtime.segment);

                        SayKit.config.remoteConfigUpdated?.Invoke();
                    }

                }

            }
        }

    }
}
