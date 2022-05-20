using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SayGames.Med.Internal;
using SayKitInternal;
using UnityEngine;


namespace SayKitInternal
{

    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
    public class SayKitBridje : MonoBehaviour
    {

        public static SayKitBridje Instance { get; private set; }

        internal static void Initialize()
        {
            SayKitDebug.Log("SayKitBridje Initialize");
            GameObject unused = new GameObject("SayKitBridje", typeof(SayKitBridje));
        }


        private void Awake()
        {
            SayKitDebug.Log("SayKitBridje Awake");

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            SayKitDebug.Log("SayKitBridje Destroy");
            if (Instance == this)
            {
                Instance = null;
            }
        }


        public void updateConversionValue(int value, int kind, string id)
        {
#if UNITY_IOS
            saykitUpdateConversionValue(value);
            AnalyticsEvent.trackEvent("skad_conversion_value", value, kind, id);
#endif
        }

#if UNITY_EDITOR
        private static void saykitUpdateConversionValue(int value) { }
#elif UNITY_IOS

        [DllImport("__Internal")]
        private static extern void saykitUpdateConversionValue(int value);

        



        public void IDFAPopupShowedEvent(string argsJson)
        {
            SayKitDebug.Log("SayKitBridje IDFAPopupShowedEvent: " + argsJson);

            var list = argsJson.ArrayListFromJson();
            var result = list[0] as String;

            RuntimeInfoManager.OnIDFAStatusResult(result);
        }

        public void IDFARedirectToSettings()
        {
            SayKitDebug.Log("SayKitBridje IDFARedirectToSettings");

            RuntimeInfoManager.DeepLinkToSettingsCalled = true;
        }
#endif
    }
}