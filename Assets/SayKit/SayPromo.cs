using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using System;
using System.Collections;
using SayKitInternal;

using System.Runtime.InteropServices;
using System.Text;

[Serializable]
class SayPromoResponse {
    public SayPromoResponseLine[] lines;
}
[Serializable]
class SayPromoResponseLine {
    public string app_title;
    public string app_store_id;
    public string app_scheme;
    public string click_url;
    public string impression_url;
    public string creative_text;
    public string creative_button;
    public string creative_type;
    public string creative_url;

    public bool _wasLoaded = false;
    public bool _wasInstalled = false;

    string _localPath = "";
    public string getLocalPath() {
        if (_localPath == "") {
            
            string extension = "";

            if (creative_type == "video") {
                extension = ".mp4";
            }
            
            _localPath = Application.persistentDataPath + "/saypromo_" + SayPromo.createMD5(creative_url) + extension;
        }
        return _localPath;
    }

    public bool isReady() {
        return !_wasInstalled && _wasLoaded;
    }
}

public class SayPromo : MonoBehaviour { 
    
    static private SayPromo instance;
    static bool wasInitialized = false;
    static bool isClickProcessing = false;
    static float loadDelay = 20f;

    static SayPromoResponseLine[] lines = {};

    static int currentLine = -1;
    static public void init() {
        
        if (wasInitialized) {
            SayKitDebug.Log("SayPromo init was started earlier.");
            return;
        }

        wasInitialized = true;

        GameObject sayPromoObject = new GameObject ("[SayPromo]");
		DontDestroyOnLoad (sayPromoObject);
        instance = sayPromoObject.AddComponent<SayPromo>();

        instance.StartCoroutine(initRoutine());   
    }

    static IEnumerator initRoutine()
    {
        // Load Config
#if UNITY_IOS
        string platform = "ios";
#elif UNITY_ANDROID
        string platform = "android";
#else
        string platform = "unknown";
#endif

        string url = "https://app.saygames.io/promo/v0?bundle=" + Application.identifier
                                                    + "&os=" + platform
                                                    + "&idfa=" + SayKit.runtimeInfo.idfa
                                                    + "&device_id=" + SayKit.runtimeInfo.idfv
                                                    + "&lang=" + SayKit.runtimeInfo.language
                                                    + "&_=" + UnityEngine.Random.Range(100000000, 900000000);
        string data = "";


        yield return new WaitUntil(() => Time.time >= loadDelay);

        SayKitDebug.Log("Getting promo config from " + url);


        string backupListJson = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                yield break;
            }
            else
            {
                backupListJson = webRequest.downloadHandler.text;
            }
        }


        SayKit.trackEvent("cross_config");
        SayKitDebug.Log("Success");
        data = backupListJson;
        SayKitDebug.Log(data);

        try
        {
            SayPromoResponse response = JsonUtility.FromJson<SayPromoResponse>(data);
            lines = response.lines;
        }
        catch (Exception exc)
        {
            SayKit.trackEventWithoutInit("sp_initRoutine_exception: ", exc.Message + " Data size: " + data?.Length + " data: " + data);

            Debug.LogError("SayPromo initRoutine data size: " + data?.Length + " json: " + data);
            Debug.LogError("SayPromo initRoutine exception: " + exc.Message);
        }

        if (lines != null)
        {
            // mark all installed & loaded in cache
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].app_scheme != "" && isAppInstalled(lines[i].app_scheme))
                {
                    lines[i]._wasInstalled = true;
                    SayKit.trackEvent("cross_installed", lines[i].app_title);
                }
                else if (System.IO.File.Exists(lines[i].getLocalPath()))
                {
                    lines[i]._wasLoaded = true;
                    SayKit.trackEvent("cross_cached", lines[i].app_title);
                }
            }

            // download 
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i]._wasInstalled)
                {
                    if (!System.IO.File.Exists(lines[i].getLocalPath()))
                    {
                        //SayKitDebug.Log("SayPromo: Downloading " + lines[i].creative_url);
                        using (UnityWebRequest creative_webRequest = UnityWebRequest.Get(lines[i].creative_url))
                        {
                            yield return creative_webRequest.SendWebRequest();

                            if (!creative_webRequest.isNetworkError)
                            {
                                try
                                {
                                    System.IO.File.WriteAllBytes(lines[i].getLocalPath(), creative_webRequest.downloadHandler.data);

                                    lines[i]._wasLoaded = true;
                                    SayKit.trackEvent("cross_downloaded", lines[i].app_title);
                                    SayKitDebug.Log("Done " + lines[i].creative_url);
                                }
                                catch { }
                            }
                        }

                    }
                    else
                    {
                        //SayKitDebug.Log("SayPromo: Already downloaded " + lines[i].creative_url);
                        lines[i]._wasLoaded = true;
                    }
                }
            }

        }

        yield break;
    }

    static public bool isAvailable() {
        if (SayKit.isPremium) {
            return false;
        }
        
        if (InternetReachabilityService.InternetReachability == NetworkReachability.NotReachable) {
            return false;
        }
        
        for(int i=0; i < lines.Length; i++) {
            if (lines[i].isReady()) {
                return true;
            }
        }
        return false;
    }

    static void nextLine() {
        int limit = 100;
        while (limit > 0) {
            currentLine++;
            if (currentLine >= lines.Length) {
                currentLine = 0;
            }
            if (lines[currentLine].isReady()) {
                break;
            }
            limit--;
        }
    }

    static public void show(VideoPlayer videoPlayer, Text title, Text button) {
        
        if (isAvailable()) {

            _skadData = "";

            nextLine();

            if (videoPlayer.isPlaying) {
                videoPlayer.Stop();
            }

            videoPlayer.url = lines[currentLine].getLocalPath();
            videoPlayer.Play();

            title.text = lines[currentLine].creative_text;
            button.text = lines[currentLine].creative_button;

            isClickProcessing = false;

            SayKit.trackEvent("cross_show", lines[currentLine].app_title);
            
            instance.StartCoroutine(TrackImpression());
        }
    }

    private static string _skadData = "";
    private static IEnumerator TrackImpression()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(lines[currentLine].impression_url))
        {
            webRequest.redirectLimit = 0;
            webRequest.timeout = 5;

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                SayKitDebug.Log("SayPromo: TrackImpression NetworkError: " + webRequest.error);
            }
            else
            {
                _skadData = webRequest.downloadHandler.text;
            }
        }

        SayKitDebug.Log("SayPromo: " + _skadData);
    }


    static public void click() {
        
        if (isClickProcessing) {
            return;
        }

        isClickProcessing = true;
        string clickId = generateClickId();

        string clickUrl = lines[currentLine].click_url + "&click_id=" + clickId;

        #if UNITY_IOS
        string storeUrl = "https://itunes.apple.com/app/apple-store/id" + lines[currentLine].app_store_id;
        #elif UNITY_ANDROID
        string storeUrl = "https://play.google.com/store/apps/details?id=" + lines[currentLine].app_store_id;
        #endif
        SayKit.trackEvent("cross_click", lines[currentLine].app_title);
        SayKit.trackEvent("cross_click_id", clickId);
        instance.StartCoroutine(clickRoutine(clickUrl, storeUrl));
    }

    static IEnumerator clickRoutine(string clickUrl, string storeUrl) {
        
        UnityWebRequest request = UnityWebRequest.Get(clickUrl);
        request.redirectLimit = 0;
        yield return request.SendWebRequest();

#if UNITY_IOS

        Int32.TryParse(lines[currentLine].app_store_id, out int storeId);
        PlatformManager.OpenStoreProductView(storeId, _skadData);

#elif UNITY_ANDROID
        Application.OpenURL(storeUrl);
#endif

        isClickProcessing = false;
        yield break;
    }

    public static string createMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    static string generateClickId() {
        const string chars= "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < 10; i++) {
            result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }
        return result.ToString();
    }

    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool sayPromoIsAppInstalled(String scheme);
    static bool isAppInstalled(String scheme) {
        return sayPromoIsAppInstalled(scheme);
    }
    #elif UNITY_ANDROID && !UNITY_EDITOR
    static bool isAppInstalled(String packageName) {
        return false;
    }
    #else
    static bool isAppInstalled(String packageName) {
        return false;
    }
    #endif
}