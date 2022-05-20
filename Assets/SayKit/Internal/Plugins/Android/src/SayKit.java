package by.saygames;

import android.app.Activity;
import android.content.Intent;
import android.os.Build;
import android.util.Log;

import com.google.android.play.core.review.ReviewInfo;
import com.google.android.play.core.review.ReviewManager;
import com.google.android.play.core.review.ReviewManagerFactory;
import com.google.android.play.core.tasks.Task;
import com.tenjin.android.TenjinSDK;

import com.facebook.FacebookSdk;
import com.facebook.appevents.AppEventsLogger;

import com.unity3d.player.UnityPlayer;

import android.preference.PreferenceManager;
import android.content.SharedPreferences;

public class SayKit {

    private static String tenjinApiKey;

    private static String _advertisingId;
    private static boolean _idfaInitialized = false;



    public static void pingFacebook(int facebookAutoEventDisabled) {
        SayKitLog.Log("w", "SayKit", "Pinging Facebook");

        //FacebookSdk.setIsDebugEnabled(true);
        //FacebookSdk.addLoggingBehavior(com.facebook.LoggingBehavior.APP_EVENTS);

        if (facebookAutoEventDisabled == 1) {
            FacebookSdk.setAutoLogAppEventsEnabled(false);
        }

        SayKitEvents.trackFullFacebookEvent("fb_mobile_activate_app", 0, "");
    }

    public static void updateAmazonMobileAds() {
        SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getActivity().getApplication());
        SharedPreferences.Editor editor = preferences.edit();
        editor.putString("aps_gdpr_pub_pref_li", "1");
        editor.apply();
    }

    static Activity getActivity() {
        return UnityPlayer.currentActivity;
    }

    public static void initTenjin(String apiKey) {
        SayKitLog.Log("w","SayKit", "Initializing Tenjin");
        tenjinApiKey = apiKey;
        TenjinSDK instance = TenjinSDK.getInstance(getActivity(), tenjinApiKey);
        instance.optIn();
        instance.connect();
    }

    public static void sendEventToTenjin(String eventName) {
        TenjinSDK instance = TenjinSDK.getInstance(getActivity(), tenjinApiKey);
        instance.eventWithName(eventName);
    }

    public static void showRateAppPopup() {
        if (android.os.Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {

            final ReviewManager reviewManager = ReviewManagerFactory.create(getActivity().getApplicationContext());
            Task<ReviewInfo> request = reviewManager.requestReviewFlow();
            request.addOnCompleteListener(task -> {
                if (task.isSuccessful()) {
                    ReviewInfo reviewInfo = task.getResult();
                    Task<Void> flow = reviewManager.launchReviewFlow(getActivity(), reviewInfo);
                    flow.addOnCompleteListener(taskFlow -> {
                        Log.i("SayKit", "Review popup was showed.");
                    });
                }
            });

        } else {
            Intent rateIntent = new Intent(getActivity(), RateAppActivity.class);
            getActivity().startActivity(rateIntent);
        }
    }

    public static void SetUnityIDFVToSayPromo(String idfv) {
        com.saypromo.core.device.Device.SetSayKitUnityIDFV(idfv);
    }



    public static String GetIDFA() {
        if(_idfaInitialized)
        {
            return _advertisingId;
        }
        else {
            return "";
        }
    }

    public static void InitializeIDFA() {

        Thread thread = new Thread(new Runnable() {
            public void run() {
                try {
                    com.saypromo.core.device.AdvertisingId.init(getActivity());

                    _advertisingId = com.saypromo.core.device.AdvertisingId.getAdvertisingTrackingId();

                    if (_advertisingId == null || _advertisingId.length() <= 0) {
                        _advertisingId = "00000000-0000-0000-0000-000000000000";
                    }
                    _idfaInitialized = true;
                } catch (Exception e) {
                    Log.e("SayKit", e.getMessage());
                }
            }
        });
        thread.start();
    }

}
