package by.saygames;

import com.amazon.device.ads.AdRegistration;

import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.fyber.inneractive.sdk.external.InneractiveAdManager;
import com.inmobi.sdk.InMobiSdk;
import com.ironsource.mediationsdk.utils.IronSourceUtils;
import com.mbridge.msdk.out.MBConfiguration;

import com.my.target.common.MyTargetVersion;
import com.ogury.sdk.Ogury;
import com.saypromo.SPAdManager;
import com.unity3d.ads.UnityAds;
import com.applovin.sdk.AppLovinSdk;

public class SayKitVersionManager {

    public static void trackSDKVersions() {
        try {

            //TODO Update sdk_adcolony
            SayKitEvents.track("sdk_adcolony", 0, 0, "4.6.3");
            SayKitEvents.track("sdk_applovin", 0, 0, AppLovinSdk.VERSION);
            //TODO Update sdk_admob
            SayKitEvents.track("sdk_admob", 0, 0, "20.3.0");
            SayKitEvents.track("sdk_facebook", 0, 0, com.facebook.ads.BuildConfig.VERSION_NAME);
            SayKitEvents.track("sdk_ironsource", 0, 0, IronSourceUtils.getSDKVersion());
            SayKitEvents.track("sdk_bytedance", 0, 0, TTAdSdk.getAdManager().getSDKVersion());
            SayKitEvents.track("sdk_unity", 0, 0, UnityAds.getVersion());
            SayKitEvents.track("sdk_vungle", 0, 0, com.vungle.warren.BuildConfig.VERSION_NAME);
            SayKitEvents.track("sdk_saypromo", 0, 0, SPAdManager.getSDKVersion());
            SayKitEvents.track("sdk_fyber", 0, 0, InneractiveAdManager.getVersion());
            SayKitEvents.track("sdk_inmobi", 0, 0, InMobiSdk.getVersion());

            //TODO Update sdk_yandex
            SayKitEvents.track("sdk_yandex", 0, 0, "4.1.0");
            SayKitEvents.track("sdk_mytarget", 0, 0, MyTargetVersion.VERSION);
            SayKitEvents.track("sdk_ogury", 0, 0, Ogury.getSdkVersion());


            String apsVersion = AdRegistration.getVersion();
            String[] apsVersionArray = apsVersion.split("-");
            apsVersion = apsVersionArray[apsVersionArray.length - 1];

            SayKitEvents.track("sdk_aps", 0, 0, apsVersion);

            String mintegralVersion = MBConfiguration.SDK_VERSION;
            String[] mintegralVersionArray = mintegralVersion.split("_");
            mintegralVersion = mintegralVersionArray[mintegralVersionArray.length - 1];

            SayKitEvents.track("sdk_mintegral", 0, 0, mintegralVersion);

        } catch (Exception e) {
            SayKitLog.Log("i", "[SayKitVersionManager]",
                    "Error getting versions ", e);
        }
    }

}
