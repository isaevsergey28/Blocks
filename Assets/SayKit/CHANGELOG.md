# Changelog
All notable changes to SayKit for Unity will be documented in this file.

## 2021-05-31
### Updated
- [iOS] All network libraries.
- [iOS] Minimal version of iOS to 10.0. 
### Added
- [iOS][Android] Setup ExternalDependencyManager automatically.  (Use SAYKIT_EDM_CHECK_DISABLE symbol to disable)
 

## 2021-07-23
### Updated
- [iOS] README
### Removed
- [iOS] Network frameworks
### Added
- [iOS][Android] ExternalDependencyManager. Disabled for Android features.


## 2021-06-08
### Added
- [iOS][Android] InApp purchase manager.

## 2021-05-31
### Updated
- [iOS][Android] All network libraries.
- [iOS][Android] All network mediations.


## 2020-08-03
### Added
- [iOS|Android] APS ad network

## 2020-07-22
### Updated
- [iOS|Android] SayKitApp.cs required params.

```
   /* Features */
    public const bool notificationsEnabled = false;
    
    /* App settings */
    public const string APP_NAME_CHINA_IOS = "<APP_NAME_CHINA_IOS or empty>";
    public const string APP_NAME_IOS = "<APP_NAME_IOS>";

    public const string APP_BUNDLE_CHINA_IOS = "<APP_BUNDLE_CHINA_IOS or empty>";
    public const string APP_BUNDLE_IOS = "<APP_BUNDLE_IOS>";


#if SAYKIT_CHINA_VERSION
    public static bool purchasesEnabled = false;

    public const string APP_KEY_IOS = "<APP_KEY_CHINA_IOS or empty>";
    public const string APP_SECRET_IOS = "<APP_SECRET_CHINA_IOS or empty>";

#else
    public static bool purchasesEnabled = true;

    public const string APP_KEY_IOS = "<APP_KEY_IOS>";
    public const string APP_SECRET_IOS = "<APP_SECRET_IOS>";

#endif
```
- Updated README. 

## 2020-05-18
### Updated
- [Android] All network libraries.
- [Android] All network mediations.

## 2020-05-04
### Added
- [Android] SayMediation supports SayPromo bidding and Mintegral ad network

## 2020-04-06
### Added
- [Android] New ad mediation

## 2020-03-25
### Updated
- [iOS] MoPub to 5.11. 
- [iOS] All network libraries.
- [iOS] All network mediations.
### Deleted
- [iOS] UIWebView.


## 2020-02-12
### Added
- [iOS|Android] Facebook events.
- [iOS|Android] Hindi localization.
- [Android] Adapter black list.
### Updated
- [Android] MoPub to 5.10. 
- [Android] All network libraries.


## 2019-12-20
### SayEndpoint
- Strict events delivery with persistent cache.

## 2019-12-18
### Updated
- [iOS] Mediation libraries.
- [Android] Tenjin sdk.

## 2019-12-10
### Fixed
- [Android] Fixed MoPub rewardedVideo bug.

## 2019-12-06
### Added
- [iOS][Android] Tenjin DeepLink log.
- [iOS][Android] interstitial_click, interstitial_close, rewarded_click, rewarded_close events.
### Updated
- [iOS|Android] Level events.

## 2019-11-29
### Fixed
- [iOS|Android] hasInterstitialCachedStatus and hasRewardedCachedStatus.
### Updated
- [iOS|Android] Updated Vungle SDK
- [Android] Saypromo to 6.0.26.
### Added
- [iOS|Android] InMobi Ads
- [iOS|Android] Debug log flag
- [Android] Exception handler
- [iOS|Android] Custom properties to Crashlytics

## 2019-11-05
### Fixed
- [Android] Fixed IronSource broadcastReceiver bug.
### Updated
- [Android] Updated Unity-Ads to 3.3.

## 2019-09-14
### Fixed
- [iOS] Fixed SayKitVersionManager bug.
### Updated
- [Android] Bidding
- [iOS] Bidding
### Added
- [iOS] Added extra to rewarded_load and interstitial_load events.
- [Android] Added extra to rewarded_load and interstitial_load events.

## 2019-09-13
### Deleted
- [Android] GameAnalytics
- [iOS] GameAnalytics
### Updated
- [iOS] saypromo to 6.5

## 2019-09-13
### Added
- [Android] Added log SDK versions.
- [Android] Updated SayPromo adapters.
- [iOS] Added log SDK versions.
- [iOS] Updated SayPromo adapters.

## 2019-09-06
### Updated
- [Android] Updated MoPub to 5.8. 
- [Android] Updated all network libraries.

## 2019-09-05
### Added
- [iOS] Waterfall tracking.

## 2019-09-04
### Fixed
- [iOS] AVPlayer bugs. 
### Updated
- [iOS] progress animation.
- [iOS] saypromo to 6.3


## 2019-08-30
### Fixed
- Storage bugs.
- [Android] MoPub network name in impression data.
- [iOS] MPSayMediationSerializeConfiguration log for MoPub adapters.
- [iOS] NSLocation warning.
### Added
- RemoteConfigManager crash handling.
### Updated
- [Android] Saypromo to 5.0.22.0. Added remote debug log.
- [iOS] Updated MoPub to 5.8. 
- [iOS] Updated all network libraries.
- [iOS] Updated saypromo to version 6.1.


## 2019-07-24
### Added
- Autoconfigurator for Android.
- `isInterstitialAvailable` method to SayKit.cs.
- SayKit folders check.
- `UNITY_CLOUD_BUILD` flag to Notifications.cs file. [iOS]
- `APP_SECRET_IOS` and `APP_SECRET_ANDROID` to SayKit.cs.
- Support last Unity versions: **2018.4.4f1** and **2018.4.4f1**
- Remote configurations for `facebook_app_id`, `facebook_app_name`, `gameAnalyticsGameKey`, `gameAnalyticsGameSecret`.

### Updated
- Library versions. [Android] 

### Migration steps:
- Delete `<uses-sdk android:minSdkVersion="16"/>` line from a _Assets/Plugins/Android/saykit/AndroidManifest.xml_ file.
- Update gradle version in a _Assets/Plugins/Android/mainTemplate.gradle_ file:

	Change `classpath 'com.android.tools.build:gradle:3.0.1'` to `classpath 'com.android.tools.build:gradle:3.2.1'`

 **[Important]** You can check all configurations in a _Assets/SayKit/Internal/Plugins/Settings_ folder or delete all android specific files if you didn't customize them. All necessary files will be configured automatically at the pre-build time.
 
- Delete `gameAnalyticsGameKey`, `gameAnalyticsGameSecret`, `FACEBOOK_APP_ID` and `FACEBOOK_APP_NAME` from a SayKitApp.cs file.
- Delete banner init from SayKitApp.cs:
 
    ```
    if (SayKit.isPremium == false) {
    	SayKit.showBanner();
    }
    ```

## 2019-06-03
### Added
- Updated MoPub framework to 5.7.

## 2018-11-26
### Added
- Changelog was added.

### Fixed
- Rate App popup crashes on Android 4-6.
