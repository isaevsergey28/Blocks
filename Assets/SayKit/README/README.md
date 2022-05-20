# SayKit for Unity

## Table of Contents
[TOC]

## General

### Remove this SDKs from your project
1. MoPub
2. External Dependency Manager
3. Any Ad Network SDK
4. Tenjin, GameAnalytics
5. Facebook SDK
6. Firebase

### Unity versions compatibility
We recommend to use Unity LTS releases with SayKit.

- Unity 2020.3 LTS
- Unity 2019.4 LTS
- Unity 2018.4 LTS

### Add SayKit files

Clone this repository into `Assets/SayKit` directory in your Unity project. **Letter case matters!**

### Configure SayKit

Make `Assets/SayKitApp.cs` file to store SayKit game specific files there.

#### Assets/SayKitApp.cs

```
using UnityEngine;
using System;

/* Remote Config */

[Serializable]
public class SayKitGameConfig {
    
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

public class SayKitApp {
	 
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
    
    
    public const string APP_KEY_ANDROID = "<APP_KEY_ANDROID>";
    public const string APP_SECRET_ANDROID = "<APP_SECRET_ANDROID>";
    
    public const string PROMO_KEY_IOS = "";

    /* App constants */
    public const string AD_INTERSTITIAL = "ad_interstitial";
    public const string AD_REWARDED = "ad_rewarded";
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void init() {
        SayKitConfig config = new SayKitConfig();

#if UNITY_IOS
		config.appKey = APP_KEY_IOS;
#elif UNITY_ANDROID
        config.appKey = APP_KEY_ANDROID;
#endif

        SayKit.init(config);
    }

}
```

## iOS Platform

### External Dependency Manager
Make sure External Dependency Manager configurated correctly:

`Assets -> External Dependency Manager -> iOS Resolver -> Settings`

Next flags should be turned on:

- Podfile Generation
- Use Shell to Execute Cocoapod Tool
- Auto Install Cocoapods Tools in Editor
- Add use_frameworks! to Podfile
- Link frameworks statically
- Always add the main target to Podfile
- Allow the same pod to be in multiple targets
- Use project settings

Open terminal and install cocoapods:

`sudo gem install cocoapods`

No special actions are required to build iOS version of project with SayKit.

## Android Platform
You don't need any special actions if it's first start. 

### Gradle and Manifest.

SayKit is using gradle and manifest template files. **Gradle Template File** and **AndroidManifest** file will be generated automatically on PreBuild.

If you need to update SayKit, you can delete gradle and manifest files. Both of them will be generated again correctly.
    
If you need to add your own modifications to this files. You can easily add it to file as new lines but make sure you haven't modificated exists lines. SayKit is checking files line by line.

### Fixes for Unity_2019.3+

1. Remove ads libraries:

    `Unity -> Window -> Package Manager -> Ads/Advertisement -> Remove`

2. SayKit is using more gradle templates: **baseProjectTemplate** and **launcherTemplate** files. It will be generated automatically on PreBuild. 

### External Dependency Manager
SayKit does not use External Dependency Manager on Android. All flags should be turned off:

`Assets -> External Dependency Manager -> Android Resolver -> Settings`


## Using SayKit

Initialization performs automatically.

You can use any public method of SayKit anytime safely.

## !!! Required Actions

- Add **SayKitUI** prefab on the top of first loading scene _(build index = 0)_.
- Add **SayKitBanner** prefab on the top of first loading scene _(build index = 0)_.
- Call `SayKit.showRateAppPopup()` after success tutorial or first level complete. Before that SayKit works in *(!) light/saving mode* to provide best possible player experience with losing in monetization at the same time. You can call `SayKit.showCustomRateAppPopup()` if you want to use your custom rate popup. 

## SayKit Object Reference

```
// State
bool isInitialized;
string BuildVersion;
SayKitLanguage getCurrentLanguage();

// Banners
void showBanner();
void hideBanner();

// Interstitial. Returns true if ads was shown.
bool showInterstitial(string place) 
bool showInterstitial(string place, Action onCloseCallback)

// Rewarded
bool isRewardedAvailable(string place)
void showRewarded(string place, Action<bool> onCloseCallback)


// Events for levels
void trackLevelStarted(int level);
void trackLevelCompleted(int level, int score);
void trackLevelFailed(int level, int score);

// Events for stages
void trackStageStarted(int stage, int level);
void trackStageCompleted(int stage, int level);
void trackStageFailed(int stage, int level);

// Events for extra levels
void trackLevelExtraStarted();
void trackLevelExtraCompleted(int score);
void trackLevelExtraFailed(int score);

// Events for chunks
void trackChunkStarted(string name, int sequenceNumber);
void trackChunkCompleted();
void trackChunkFailed();


// Events for items unlock/receive
void trackItem(string item);

// Event for opened screen
void trackScreen(string screen)

//Event for clicked element on screen
void trackClick(string screen, string element)

// Track soft and hard currencies
void trackSoftIncome(int amount, int total)
void trackSoftOutcome(int amount, int total)
void trackHardIncome(int amount, int total)
void trackHardOutcome(int amount, int total)
void trackSoftIncome(int amount, int total, string place)
void trackSoftOutcome(int amount, int total, string place)
void trackHardIncome(int amount, int total, string place)
void trackHardOutcome(int amount, int total, string place)


// Events
void trackEvent(string eventName)
void trackEvent(string eventName, int eventParam)
void trackEvent(string eventName, int eventParam1, int eventParam2)
void trackEvent(string eventName, string eventParam)
void trackEvent(string eventName, int eventParam1, string eventParam2, string eventParam3)
void trackEvent(string eventName, int eventParam1, int eventParam2, string eventParam3, string eventParam4)


// In-App Purchases
void SayKit.trackPurchase(PurchaseEventArgs purchaseEventArgs)
void SayKit.enablePremium()
bool SayKit.isPremium

// Rate App. Returns true if popup was shown. Popup is shown once during lifetime of the app. 
bool SayKit.showRateAppPopup()

// GDPR
bool? isGdprApplicable()
void revokeGdprConsent()
```


## GDPR

Place `SayKitUI` prefab on the top in the first loaded scene. SayKit will automatically determine if user is the subject of the GDPR and will show popup to obtain consent.

Make sure clicks on the scene are allowed!

Also you should place **Withdraw Concent** button in settings menu, that calls `SayKit.revokeGdprConsent()`

Snow this button only when `SayKit.isGdprApplicable()` is true.

## Remote Config

`SayKitGameConfig` is the linear root structure filled by SayKit when it loads. 

Example:

```
[Serializable]
public class SayKitGameConfig {
    
    public int some_param = 0;
    public float other_param = 2;
    public string string_param = "";
    
}
```

After that you can implement your own methods to find and work with values in arrays.

## Tracking events for Analytics

### Levels

```
SayKit.trackLevelStarted(int level);
SayKit.trackLevelCompleted(int level, int score)
SayKit.trackLevelFailed(int level, int score)
```

`level` is number of level that the user sees in game. Starts with `1`.  
`score` could be some points, number of stars or `0` if not needed.

If levels have internal IDs, you can use similar function to additionaly tag each level:

```
SayKit.trackLevelStarted(string tag, int level);
SayKit.trackLevelCompleted(string tag, int level, int score)
SayKit.trackLevelFailed(string tag, int level, int score)
```

### Levels with Stages

When level contains several stages to complete, you could use extra functions:

```
SayKit.trackLevelStageStarted(int number)
SayKit.trackLevelStageCompleted(int number, int score)
SayKit.trackLevelStageFailed(int number, int score)
```

`number` is number of stage. Starts with `1`.  

The order of the calls is important. First goes `trackLevelStarted`, then stages, finally `trackLevelCompleted` or `trackLevelFailed`. Example:

1. `SayKit.trackLevelStarted(5);`
2. `SayKit.trackLevelStageStarted(1);`
3. *Stage 1 is played*
4. `SayKit.trackLevelStageCompleted(1, 0)`
5. `SayKit.trackLevelStageStarted(2)`
6. *Stage 2 is played*
6. `SayKit.trackLevelStageCompleted(2, 0)`
7. `SayKit.trackLevelStageStarted(3)`
8. `SayKit.trackLevelStageFailed(3)`
9. `SayKit.trackLevelFailed(5, 0)`

Function for tagged stages:

```
SayKit.trackLevelStageStarted(string tag, int number)
SayKit.trackLevelStageCompleted(string tag, int number, int score)
SayKit.trackLevelStageFailed(string tag, int number, int score)
```

### Extra Levels (Bonus Levels, Boss Levels, ...)

Extra level doesn't increase number of passed levels in game.

```
SayKit.trackLevelExtraStarted()
SayKit.trackLevelExtraCompleted(int score)
SayKit.trackLevelExtraFailed(int score)
```

Function for tagged extra levels:

```
SayKit.trackLevelExtraStarted(string tag)
SayKit.trackLevelExtraCompleted(string tag, int score)
SayKit.trackLevelExtraFailed(string tag, int score)
```

Example:

1. `SayKit.trackLevelStarted(5)`
2. `SayKit.trackLevelCompleted(5, 0)`
3. `SayKit.trackLevelExtraStarted("red_boss")`
4. `SayKit.trackLevelExtraCompleted("red_boss", 0)`


## In-App Purchases

- Add `SAYKIT_PURCHASING` in *Player Settings* > *Other Settings* > *Scripting Define Symbols* for every platform.
- Set constant `SayKitApp.purchasesEnabled` to `true`.
- Add **In App Purchasing** package from Package Manager.

When you process IAPs, call `SayKit.trackPurchase()` for every successful purchase.

### No ADS

Call `SayKit.enablePremium()` when user successfully purchased _No Ads_. Then SayKit will hide banner and stop serve banners, interstitials and cross-promo.

You can check if user has premium status by `SayKit.isPremium` variable.

## Push Notifications

- Add `SAYKIT_NOTIFICATIONS` in *Player Settings* > *Other Settings* > *Scripting Define Symbols* for every platform.
- Set constant `SayKitApp.notificationsEnabled` to `true`.

SayKit will request user permission (iOS only) for sending notification on app launch and save token for sending server-side push notifications.


## No Banners

- Add `SAYKIT_BANNER_DISABLED` in *Player Settings* > *Other Settings* > *Scripting Define Symbols* for every platform.


## China config

- Choose `China config` from SayKit menu for China only distribution *Menu* > *SayKit* > *Application config* > *China config*
- Choose `World config`  from SayKit menu for worldwide distribution.
- Use `SAYKIT_CHINA_VERSION` symbol to process different kinds of cases.

## Unity Cloud Build

No special actions are required to build Android version using Unity Cloud Build.

For iOS platform: Open Unity Dashboard page in browser > *Cloud Build* -> *Config* -> *Your build target name*-> *Advanced options*. 

- Add `Assets/SayKit/UnityCloudBuild/firebase_upload_symbols.json` to *Custom Fastlane Configuration Path* in advanced options.

Also add `SAYKIT_UPLOAD_SYMB_DISABLE` flag in *Player Settings* > *Other Settings* > *Scripting Define Symbols* for iOS platform. It will disable uploading dsyms on build.
 
