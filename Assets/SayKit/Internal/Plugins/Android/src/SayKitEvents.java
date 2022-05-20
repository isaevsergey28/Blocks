package by.saygames;

import android.app.Activity;
import android.app.ActivityManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.NonNull;

import com.facebook.appevents.AppEventsLogger;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.crashlytics.FirebaseCrashlytics;
import com.google.firebase.installations.FirebaseInstallations;
import com.unity3d.player.UnityPlayer;
import org.json.JSONObject;

import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.util.Arrays;
import java.util.Calendar;
import java.text.SimpleDateFormat;
import java.lang.Math;
import java.util.Currency;
import java.util.HashSet;
import java.util.List;

import com.google.firebase.analytics.FirebaseAnalytics;


import static android.content.Context.ACTIVITY_SERVICE;

public class SayKitEvents {

    private static FirebaseAnalytics mFirebaseAnalytics;
    private static AppEventsLogger mFacebookEventsLogger;

    private static ActivityManager mActivityManager;


    static SayKitEvents instance;
    static SayKitEvents getInstance() {
        if (instance == null) {
            SayKitLog.Log("w","SayKit", "Instantiate SayKitEvents");
            instance = new SayKitEvents();
            instance.endpoint.open();
            instance.endpoint.addSslPin("*.saygames.io", "sha256/4a6cPehI7OG6cuDZka5NDZ7FR8a60d3auda+sKfg4Ng=");
            instance.endpoint.autoFlush(5 * 1000);
        }
        return instance;
    }

    public static void init(String appKey, String version, String appVersion, int track_waterfall) {

        getInstance();
        mFirebaseAnalytics = FirebaseAnalytics.getInstance(getActivity());
        mFacebookEventsLogger = AppEventsLogger.newLogger(getActivity());
        mActivityManager = (ActivityManager)  getActivity().getSystemService(ACTIVITY_SERVICE);

        SayExceptionHandler.initUncaughtExceptionHandler();

        FirebaseCrashlytics.getInstance().setCustomKey("device", Build.MANUFACTURER + "+" + Build.MODEL);
        FirebaseCrashlytics.getInstance().setCustomKey("start_time", dateFormat.format(Calendar.getInstance().getTime()));


        int versionCode = getAppVersion();
        SayKitEvents.track("android_build", versionCode, 0, "" + versionCode);


    }

    static Activity getActivity() {
        return UnityPlayer.currentActivity;
    }

    public static void trackFull(String appKey, String idfa, String device_id, String device_os, String device_name, String version, int segment, String eventName, int param1, int param2, String extra, int param3, int param4, String extra2, String tag, int level, int scurrency, int hcurrency) {
        SayKitEvents instance = getInstance();

        synchronized (instance) {
            instance.appKey = appKey;
            instance.idfa = idfa;
            instance.device_id = device_id;
            instance.device_os = device_os;
            instance.device_name = device_name;

            if (segment > 0) {
                instance.version = version + "." + segment;
            } else {
                instance.version = version;
            }

            instance.segment = segment;
            instance.level = level;
            instance.scurrency = scurrency;
            instance.hcurrency = hcurrency;
        }

        FirebaseCrashlytics.getInstance().setUserId(device_id);

        instance.endpoint.setUrl("https://track.saygames.io/events/" + appKey);
        instance.trackEvent(eventName, param1, param2, extra, param3, param4, extra2, tag, false);
    }

    public static void track(String eventName, int param1, int param2, String extra) {
        getInstance().trackEvent(eventName, param1, param2, extra, 0, 0 ,"", "", false);
    }

    public static void trackImmediately(String eventName, int param1, int param2, String extra) {
        getInstance().trackEvent(eventName, param1, param2, extra, 0, 0 ,"", "", true);
        getInstance().endpoint.flushRequests();
    }

    public static void trackFirebaseEvent(String eventName, String extra) {
        Bundle params = new Bundle();
        params.putString("extra", extra);
        mFirebaseAnalytics.logEvent(eventName, params);
    }

    public static void setCrashlyticsParam(String paramName, String paramValue) {
        FirebaseCrashlytics.getInstance().setCustomKey(paramName, paramValue);
    }



    public static void trackFirebaseEventWithValue(String eventName, float extra) {
        Bundle params = new Bundle();
        params.putFloat(FirebaseAnalytics.Param.VALUE, extra);

        if (eventName.equals("ads_earnings")) {
            params.putString(FirebaseAnalytics.Param.CURRENCY, "USD");
        }

        mFirebaseAnalytics.logEvent(eventName, params);
    }

    public static void trackFullFirebaseEvent (String logEvent, float valueToSum, String customJSPN) {
        Bundle params = convertCustomJSPNToDictionary(customJSPN);

        if ( valueToSum != 0) {
            params.putFloat(FirebaseAnalytics.Param.VALUE, valueToSum);
        }

        mFirebaseAnalytics.logEvent(logEvent, params);
    }



    public static void trackFacebookEvent (String eventName, String extra) {
        Bundle params = new Bundle();
        params.putString("extra", extra);
        params.putString("version", "a2");

        mFacebookEventsLogger.logEvent(eventName, params);
    }

    public static void trackFacebookPurchaseEvent(float valueToSum, String currencyCode ) {
        Bundle params = new Bundle();
        params.putString("fb_currency", currencyCode);
        params.putString("version", "a2");

        mFacebookEventsLogger.logEvent("fb_mobile_purchase", valueToSum, params);
    }

    public static void trackFullFacebookEvent (String logEvent, float valueToSum, String customJSPN) {

        Bundle params = convertCustomJSPNToDictionary(customJSPN);
        params.putString("version", "a2");

        if(valueToSum == 0)
        {
            mFacebookEventsLogger.logEvent(logEvent, params);
        }
        else {
            mFacebookEventsLogger.logEvent(logEvent, valueToSum, params);
        }

    }


    public static int ApplicationStartTimestamp = -1;
    public static int getApplicationStartTimestamp() {
        return ApplicationStartTimestamp;
    }


    public static Bundle convertCustomJSPNToDictionary (String customJSPN) {

        Bundle params = new Bundle();

        try {
            if (customJSPN.length() > 0) {
                while (customJSPN.length() > 0) {
                    int part = customJSPN.indexOf("%|%");

                    String key = customJSPN.substring(0, part);
                    customJSPN = customJSPN.substring(part + 3);

                    part = customJSPN.indexOf("%|%");
                    String value = customJSPN.substring(0, part);
                    customJSPN = customJSPN.substring(part + 3);

                    part = customJSPN.indexOf("}&%&{");
                    String typeValue = customJSPN.substring(0, part);
                    customJSPN = customJSPN.substring(part + 5);


                    if (typeValue.equals("bool")) {
                        params.putBoolean(key, Boolean.parseBoolean(value));
                    } else if (typeValue.equals("int")) {
                        params.putInt(key, Integer.parseInt(value));
                    } else if (typeValue.equals("float")) {
                        params.putFloat(key, Float.parseFloat(value));
                    } else {
                        params.putString(key, value);
                    }

                }
            }
        } catch (Exception exc) {
            SayKitLog.Log("e", "Error", exc.getMessage());
        }

        return params;
    }




    //RequestQueue requestQueue;

    public String appKey = "";
    public String idfa = "";
    public String device_id = "";
    public String device_os = "";
    public String device_name = "";
    String version = "";
    int segment = 0;
    int level = 0;
    int scurrency = 0;
    int hcurrency = 0;

    int sequence = 0;

    static String session = "";
    static long sessionUpdatedAt = 0;

    //StringBuffer buffer = new StringBuffer();
    SayEndpoint endpoint = new SayEndpoint(getActivity(), "SayKitEvents", SayEndpoint.getDefaultHandler());

    static String getSession() {

        long now = Calendar.getInstance().getTimeInMillis();

        if (now - sessionUpdatedAt > 120*1000) {
            String letters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
            int length = 16;

            session = "";
            for (int i = 0; i < length; i++) {
                int k = (int) ( Math.random()*letters.length());
                session = session + letters.charAt(k);
            }

            FirebaseCrashlytics.getInstance().setCustomKey("session", session);
        }

        sessionUpdatedAt = now;
        return session;
    }

    static SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");

    void trackEvent(String eventName, int param1, int param2, String extra, int param3, int param4, String extra2, String tag, boolean priority) {
        JSONObject eventJson = new JSONObject();
        String timeStr = dateFormat.format(Calendar.getInstance().getTime());

        synchronized (this) {
            sequence++;

            try {
                eventJson.put("time", timeStr);
                eventJson.put("sequence", sequence);
                eventJson.put("session", getSession());
                eventJson.put("idfa", idfa);
                eventJson.put("device_id", device_id);
                eventJson.put("device_os", device_os);
                eventJson.put("device_name", device_name);
                eventJson.put("version", version);
                eventJson.put("event", eventName);

                eventJson.put("tag", tag);

           	    eventJson.put("param1", param1);
                eventJson.put("param2", param2);
                eventJson.put("extra", extra);
                eventJson.put("param3", param3);
                eventJson.put("param4", param4);
                eventJson.put("extra2", extra2);

                eventJson.put("level", level);
                eventJson.put("scurrency", scurrency);
                eventJson.put("hcurrency", hcurrency);
            } catch (Exception e) {
                //TODO: handle exception
            }
        }


        SayKitLog.Log("w","SayKit", eventJson.toString());

        priority = priority || isPriorityEvent(eventName);

        endpoint.addBatchRequest(eventJson.toString(), SayEndpoint.batch.appendWithNewLine, priority);
    }

    private static final HashSet<String> PRIORITY_EVENTS = new HashSet<>(Arrays.asList(
        "app_start",
        "unity_engine",
        "crash_report",
        "level_completed",
        "level_failed",
        "level_started",
        "bonus_level_completed",
        "bonus_level_started",
        "bonus_level_failed",
        "iap_android",
        "interstitial_imp",
        "rewarded_imp"
    ));

    private static final String[] PRIORITY_PREFIXES = {
            "imp_",
            "ltv_"
    };

    private boolean isPriorityEvent(String eventName) {
        if  (PRIORITY_EVENTS.contains(eventName)) {
            return true;
        }
        for (String prefix : PRIORITY_PREFIXES) {
            if (eventName.startsWith(prefix)) {
                return true;
            }
        }

        return  false;
    }


    private static Boolean mTotalMemoryTracked = false;
    private static void trackTotalMemory() {
        try {
            if (mActivityManager != null) {
                ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
                mActivityManager.getMemoryInfo(mi);

                int totalMb = (int) (((double) mi.totalMem) / 1048576);

                track("total_memory", totalMb, 0, "");
                mTotalMemoryTracked = true;
            }
        }
        catch (Exception exc)
        {
            mTotalMemoryTracked = true;
            SayKitLog.Log("e", "Error", exc.getMessage());
        }
    }

    public static void trackAvailableMemory() {
        try {

            if(!mTotalMemoryTracked)
            {
                trackTotalMemory();
            }

            if (mActivityManager != null) {
                ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
                mActivityManager.getMemoryInfo(mi);

                int availableMb = (int)(((double) mi.availMem) / 1048576);

                track("free_memory", availableMb, 0, "");
            }
        }
        catch (Exception exc)
        {
            SayKitLog.Log("e", "Error", exc.getMessage());
        }
    }


    private static int getAppVersion() {
        try {
            String pkgName = UnityPlayer.currentActivity.getApplicationContext().getPackageName();

            Class<?> buildConfigClass = Class.forName(pkgName + ".BuildConfig");
            Object buildConfigInstance = buildConfigClass.newInstance();

            Field field = buildConfigClass.getDeclaredField("VERSION_CODE");
            field.setAccessible(true);

            return field.getInt(buildConfigInstance);
        } catch (Exception e) {
            SayKitLog.Log("i", "[SayKitVersion]",
                    "Error getting VERSION_CODE ", e);
            return 0;
        }
    }


    public static void trackSDKVersions()
    {
        SayKitVersionManager.trackSDKVersions();
    }

    public static void trackFirebaseId() {

        FirebaseAnalytics.getInstance(getActivity()).getAppInstanceId().addOnCompleteListener(new OnCompleteListener<String>() {
            @Override
            public void onComplete(@NonNull Task<String> task) { try {
                String firebaseId = task.getResult();
                track("firebase_client_id", 0, 0, firebaseId);
            }
            catch (Exception e)
            {
                SayKitLog.Log("i", "[trackFirebaseId]",
                        "Error getting firebase_id ", e);
            }
            }
        });
    }

}
