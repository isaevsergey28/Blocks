using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;


namespace SayKitInternal {

    class PlatformManager {

        public static SayKitLanguage CurrentLanguage = SayKitLanguage.English;

#if UNITY_EDITOR

        private static void sayKitPingFacebook(int facebookAutoEventDisabled) {}
        private static void sayKitUpdateAmazonMobileAds() {}
        private static void sayKitShowRateAppPopup() {}
        private static float sayKitBottomSafePadding() {
            return 0;
        }
        private static void sayKitCrashlyticsInit() {}
        
        private static float sayKitScreenScale() {
            return 1;
        }

        private static string sayKitGetSystemLanguage() { return ""; }
        private static string sayKitGetIDFA() { return "00000000-0000-0000-0000-000000000000"; }



        public static void sayKitShowSystemIDFAPopup() { }

        public static void sayKitShowNativeIDFAPopup(string title, string description, string okBtnString, string cancelBtnString) { }


        public static string sayKitGetCurrentIDFAStatus() { return "ATTrackingManagerAuthorizationStatusNotDetermined"; }


        private static void sayKitSetUnityIDFVToSayPromo(string idfv) { }

        private static void sayKitGetInitializeIDFA() { }


        private static void sayKitOpenStoreProductView(int store_id, string skadData) { }

#elif UNITY_IOS

        [DllImport("__Internal")]
        private static extern void sayKitPingFacebook(int facebookAutoEventDisabled);

        [DllImport("__Internal")]
        private static extern void sayKitUpdateAmazonMobileAds();
        
        [DllImport("__Internal")]
        private static extern void sayKitShowRateAppPopup();

        [DllImport("__Internal")]
        private static extern float sayKitBottomSafePadding();

        [DllImport("__Internal")]
        private static extern float sayKitScreenScale();
        
        [DllImport("__Internal")]
        private static extern void sayKitCrashlyticsInit();

        [DllImport("__Internal")]
        private static extern string sayKitGetSystemLanguage();

        [DllImport("__Internal")]
        private static extern string sayKitGetIDFA();


        [DllImport("__Internal")]
        public static extern string sayKitGetCurrentIDFAStatus();


        [DllImport("__Internal")]
        public static extern void sayKitShowSystemIDFAPopup();

        [DllImport("__Internal")]
        public static extern void sayKitShowNativeIDFAPopup(string title, string description, string okBtnString, string cancelBtnString);


        [DllImport("__Internal")]
        private static extern void sayKitOpenStoreProductView(int store_id, string skadData);

#elif UNITY_ANDROID
        
        private static readonly AndroidJavaClass SayKitJava = new AndroidJavaClass("by.saygames.SayKit");
        
        private static void sayKitPingFacebook(int facebookAutoEventDisabled) {
            SayKitJava.CallStatic("pingFacebook", facebookAutoEventDisabled);
        }

        private static void sayKitUpdateAmazonMobileAds() {
            SayKitJava.CallStatic("updateAmazonMobileAds");
        }

        private static void sayKitShowRateAppPopup() {
            SayKitJava.CallStatic("showRateAppPopup");
        }

        private static float sayKitBottomSafePadding() {
            return 0;
        }

        private static float sayKitScreenScale() {
            return Screen.dpi / 160f;
        }

        private static void sayKitCrashlyticsInit() {
            // Firebase init automaticaly
        }

        public static string sayKitGetCurrentIDFAStatus() {
            return "";
        }


        private static void sayKitSetUnityIDFVToSayPromo(string idfv) {
            SayKitJava.CallStatic("SetUnityIDFVToSayPromo", idfv);
        }

        private static string sayKitGetIDFA() {
            return SayKitJava.CallStatic<string>("GetIDFA");
        }

        private static void sayKitGetInitializeIDFA() {
            SayKitJava.CallStatic("InitializeIDFA");
        }

#endif


        static public void pingFacebook(int facebookAutoEventDisabled) {
            sayKitPingFacebook(facebookAutoEventDisabled);
        }

        static public void initAfterGdpr(bool inAppPurchaseServerCheck)
        {
            var facebookAutoEventDisabledFlag = inAppPurchaseServerCheck ? 1 : 0;
            pingFacebook(facebookAutoEventDisabledFlag);

            sayKitUpdateAmazonMobileAds();
        }

        static public bool showRateAppPopup() {
            if (Storage.instance.rateAppPopupWasShowed == false) {
                Storage.instance.rateAppPopupWasShowed = true;
                Storage.instance.save();

                sayKitShowRateAppPopup();
                return true;
            }
            return false;
        }

        static public bool showCustomRateAppPopup() {
            if (Storage.instance.rateAppPopupWasShowed == false) {
                Storage.instance.rateAppPopupWasShowed = true;
                Storage.instance.save();
                return true;
            }
            return false;
        }

        static public float bottomSafePadding() {
            return sayKitBottomSafePadding();
        }

        static public float screenScale() {
            return sayKitScreenScale();
        }

        static public void crashlyticsInit() {
            sayKitCrashlyticsInit();
        }


        static public string getNativeSystemLanguage()
        {

#if UNITY_ANDROID
            try
            {
                var locale = new AndroidJavaClass("java.util.Locale");
                var localeInst = locale.CallStatic<AndroidJavaObject>("getDefault");
                var name = localeInst.Call<string>("getLanguage");

                return name;
            }
            catch (System.Exception ex)
            {
                SayKitDebug.Log(ex.Message);

                return "";
            }
#else
            return sayKitGetSystemLanguage();
#endif
        }

        static public string getIDFA() {
            return sayKitGetIDFA();
        }

        static public string getBuildVersion()
        {
            if (SayKit.isInitialized)
            {
                return SayKit.remoteConfig.runtime.segment == 0 ? SayKit.runtimeInfo.version
                    : SayKit.runtimeInfo.version + "." + SayKit.remoteConfig.runtime.segment;
            }
            else
            {
                return "";
            }
        }

        public static void SetUnityIDFVToSayPromo(string idfv)
        {
#if UNITY_ANDROID
            sayKitSetUnityIDFVToSayPromo(idfv);
#endif
        }

        public static void InitializeIDFAAndroid()
        {
#if UNITY_ANDROID
            sayKitGetInitializeIDFA();
#endif
        }

        public static void OpenStoreProductView(int storeId, string skadData)
        {
#if UNITY_ANDROID
// not using
#else
            sayKitOpenStoreProductView(storeId, skadData);
#endif
        }
    }
}