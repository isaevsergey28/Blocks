using System;
using System.Collections.Generic;
using System.IO;
using SayKitInternal;
using UnityEngine;

public class SayKitRemoteSettings
{
    public static SayKitRemoteSettings SharedInstance = new SayKitRemoteSettings();

    [Serializable]
    public class SayKitRemoteConfiguration
    {
        public string Name;
        public string Data;
    }

    [Serializable]
    public class SayKitRemoteData
    {
        public int Version;
        public string Error;
        public string Platform;
        public List<SayKitRemoteConfiguration> Configuration;
    }

    public static string buildPlace = "default";

    public string GetRemoteURL()
    {
        string url = "https://api.launcher.saygames.io/saykit/configure?";

#if UNITY_IOS
            url += "app_key=" + SayKitApp.APP_KEY_IOS;
            url += "&app_secret=" + SayKitApp.APP_SECRET_IOS;
            url += "&saykit_platform=" + "ios";
#elif UNITY_ANDROID
        url += "app_key=" + SayKitApp.APP_KEY_ANDROID;
        url += "&app_secret=" + SayKitApp.APP_SECRET_ANDROID;
        url += "&saykit_platform=" + "android";
#endif

        url += "&app_version=" + Application.version;
        url += "&saykit_version=" + SayKit.GetVersion;

        url += "&place=" + buildPlace;
        url += "&device_id=" + SystemInfo.deviceUniqueIdentifier;

        return url;
    }

    public SayKitRemoteSettings()
    {
        string url = GetRemoteURL();

        SayKitWebRequest sayKitWebRequest = new SayKitWebRequest(url);
        sayKitWebRequest.SendAndWait(10);

        String data = "";

        try
        {
            if (sayKitWebRequest.IsDone && string.IsNullOrEmpty(sayKitWebRequest.ErrorMessage))
            {
                if (sayKitWebRequest.Text.Length > 0 && sayKitWebRequest.Text[0] == '{')
                {
                    data = sayKitWebRequest.Text;
                    Debug.LogWarning("Config data was downloaded!");
                }
            }
        }
        catch { }




        var config = JsonUtility.FromJson<SayKitRemoteData>(data);

        if (config.Error.Length == 0)
        {
            for (int i = 0; i < config.Configuration.Count; i++)
            {
                switch (config.Configuration[i].Name)
                {
                    case "facebook_app_id":
                        facebook_app_id = config.Configuration[i].Data;
                        break;
                    case "facebook_app_name":
                        facebook_app_name = config.Configuration[i].Data;
                        break;
                    case "gameanalytics_key":
                        gameAnalyticsGameKey = config.Configuration[i].Data;
                        break;
                    case "gameanalytics_secret":
                        gameAnalyticsGameSecret = config.Configuration[i].Data;
                        break;
                    case "admob_app_id":
                        admob_app_id = config.Configuration[i].Data;
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("saykit_settings file is missed! Please, check a Assets/Resources/saykit_settings.json");
        }


        if (String.IsNullOrEmpty(facebook_app_name)
                || String.IsNullOrEmpty(facebook_app_id))
        {
            Debug.LogError("saykit_settings file doesn't contain configuration data! Please, check a Assets/Resources/saykit_settings.json");
        }
        else
        {
            SayKitDebug.Log("saykit_settings is successfully initialized. "
                + "\n facebook_app_id: " + facebook_app_id
                + "\n facebook_app_name: " + facebook_app_name);
        }

    }

    public string facebook_app_id = "";
    public string facebook_app_name = "";
    public string gameAnalyticsGameKey = "";
    public string gameAnalyticsGameSecret = "";

    public string admob_app_id = "";

}
