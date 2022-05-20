using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class SayKitDebug
{
    private static bool DebugFlag = false;


#if UNITY_EDITOR
    private static void sayKitLogSetFlag(int debugFlag) { }
#elif UNITY_IOS
    [DllImport("__Internal")]
    private static extern void sayKitLogSetFlag(int debugFlag);
#elif UNITY_ANDROID
    private static readonly AndroidJavaClass SayKitLogJava = new AndroidJavaClass("by.saygames.SayKitLog");

    private static void sayKitLogSetFlag(int debugFlag) {
       SayKitLogJava.CallStatic("SetDebugFlag", debugFlag);
    }
#endif


    public static void InitDebugLogs()
    {
#if SAYKIT_DEBUG
		DebugFlag = true;
        sayKitLogSetFlag(1);
#else
        DebugFlag = Convert.ToBoolean(PlayerPrefs.GetInt("sayKitDebugFlag"));
        sayKitLogSetFlag(PlayerPrefs.GetInt("sayKitDebugFlag"));
#endif


    }


    public static void Log(string message)
    {
        if (DebugFlag)
        {
            Debug.Log(message);
        }
    }

    public static void LogFormat(string format, params object[] args)
    {
        if (DebugFlag)
        {
            Debug.LogFormat(format, args);
        }
    }

    [Serializable]
    public class SayKitUnityException
    {
        public string scene;
        public string exception;
    }

    private static int _lastEventTimestamp = 0;
    public static void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            if (SayKitInternal.Utils.currentTimestamp - _lastEventTimestamp > 5)
            {
                _lastEventTimestamp = SayKitInternal.Utils.currentTimestamp;

                try
                {
                    var unityException = new SayKitUnityException
                    {
                        scene = SceneManager.GetActiveScene().name,
                        exception = condition
                    };

                    var extra1 = JsonUtility.ToJson(unityException);
                    SayKitInternal.AnalyticsEvent.trackEvent("unity_exception", extra1, stackTrace);
                }
                catch (Exception exc)
                {
                    Log(exc.Message);
                }
            }
        }
    }

}
