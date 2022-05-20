using UnityEngine;
using System;

/* Remote Config */

[Serializable]
public class SayKitGameConfig
{

    /* Place remote configs parameters here as class properties with default values. */

    public int some_param = 0;
    public float other_param = 2;
    public string string_param = "";

    /*
    You can access them using:
    
    SayKit.gameConfig.some_param;
    SayKit.gameConfig.other_param;
    SayKit.gameConfig.string_param;
    */
}

public class SayKitApp
{

    /* Features */
    public const bool notificationsEnabled = false;


    /* App settings */
    public const string APP_NAME_CHINA_IOS = "<APP_NAME_CHINA_IOS or empty>";
    public const string APP_NAME_IOS = "Block Crafter (Android)";

    public const string APP_BUNDLE_CHINA_IOS = "<APP_BUNDLE_CHINA_IOS or empty>";
    public const string APP_BUNDLE_IOS = "block.crafter.build";


#if SAYKIT_CHINA_VERSION
    public static bool purchasesEnabled = false;

    public const string APP_KEY_IOS = "<APP_KEY_CHINA_IOS or empty>";
    public const string APP_SECRET_IOS = "<APP_SECRET_CHINA_IOS or empty>";

#else
    public static bool purchasesEnabled = false;

    public const string APP_KEY_IOS = "<APP_KEY_IOS>";
    public const string APP_SECRET_IOS = "<APP_SECRET_IOS>";

#endif


    public const string APP_KEY_ANDROID = "blkcrfa";
    public const string APP_SECRET_ANDROID = "Q0e26malAdUhOXHV42UBItwPKU4n7fyM";

    public const string PROMO_KEY_IOS = "";

    /* App constants */
    public const string AD_INTERSTITIAL = "ad_interstitial";
    public const string AD_REWARDED = "ad_rewarded";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void init()
    {
        SayKitConfig config = new SayKitConfig();

#if UNITY_IOS
		config.appKey = APP_KEY_IOS;
#elif UNITY_ANDROID
        config.appKey = APP_KEY_ANDROID;
#endif

        SayKit.init(config);
    }

}
