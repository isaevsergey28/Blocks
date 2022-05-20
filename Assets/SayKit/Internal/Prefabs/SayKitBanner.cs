
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SayKitBanner : MonoBehaviour {
    
    private static SayKitBanner instance;
    Image image;
  
    public static SayKitBanner getInstance() {
        if (instance == null) {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++) {
                if (rootObjects[i].GetComponent<SayKitBanner>() != null) {
                    rootObjects[i].name = "[SayKitBanner]";
                    DontDestroyOnLoad(rootObjects[i]);
                    instance = rootObjects[i].GetComponent<SayKitBanner>();

                    instance.image = instance.GetComponentInChildren<Image>();

                    instance.image.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                    instance.image.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
                    instance.image.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                    instance.image.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    instance.image.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

                    break;
                }
            }
        }

        #if UNITY_EDITOR
        if (instance == null) {
            string message = "Please, add SayKitBanner prefab on the top of first loading scene (build index = 0).";
            EditorUtility.DisplayDialog("SayKit Message", message, "OK");
            throw new System.Exception("SayKit: " + message);
        }
        #endif

        return instance;

    }

    public static void Show(float height) {
        var i = getInstance();
        if (i != null) {
            i.InternalShow(height);
        }
    }

    public static void Hide() {
        var i = getInstance();
        if (i != null) {
            i.InternalHide();
        }
    }

    void InternalShow(float height) {

        var bottomOffset = SayKitInternal.PlatformManager.bottomSafePadding();
        var scale = SayKitInternal.PlatformManager.screenScale();
        
        var resultHeight = (bottomOffset + height + SayKit.remoteConfig.ads_settings.banner_bg_padding)*scale;
        if (resultHeight < 0) {
            resultHeight = 0;
        }
        
        Color color = new Color(1f, 1f, 1f);
        if (SayKit.remoteConfig.ads_settings.banner_bg_color.Length > 0) {
            ColorUtility.TryParseHtmlString(SayKit.remoteConfig.ads_settings.banner_bg_color, out color);
        }
        
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(0, resultHeight);
        image.GetComponent<Image>().color = color;
        image.gameObject.SetActive(true);
    }

    void InternalHide() {
        image.gameObject.SetActive(false);
    }
}