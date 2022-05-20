#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
using SayKitInternal;


public class ApplicationSettings
{
    public static string facebook_app_id;
    public static string facebook_app_name;
    public static string admob_app_id;
}

public class SayKitPreBuild : IPreprocessBuildWithReport
{

    const string DIALOG_TITLE = "SayKit Build";

    public bool dirtyFlag = false;

    public int callbackOrder { get { return 0; } }

    public static string buildPlace = "default";



    public void OnPreprocessBuild(BuildReport report)
    {
        OnPreprocessBuild(report, false);
    }

    interface IPreBuildTask
    {
        string Title();
        string Run();
    }

    class EmbedConfig : IPreBuildTask
    {

        public EmbedConfig(string appKey)
        {
            this.appKey = appKey;
        }

        string appKey = "";

        public string Title() { return "Embedding config"; }
        public string Run()
        {
            string result = "";

            if (appKey != "")
            {

                string path = "Assets/Resources";
                string key = path + "/saykit_" + appKey + "_" + Application.version + ".json";
                string url = "https://app.saygames.io/config/" + appKey + "?version=" + Application.version + "&_=" + UnityEngine.Random.Range(100000000, 900000000);

                string errorTemplate = "Can't embed config for " + appKey + " version " + Application.version + "\n\nUrl: " + url + "\n\nLocal path: " + key + "\n\n";


                SayKitWebRequest sayKitWebRequest = new SayKitWebRequest(url);
                sayKitWebRequest.SendAndWait(10);

                string data = sayKitWebRequest.Text;
                if (data.Length > 0 && data[0] == '{')
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        System.IO.File.WriteAllText(key, data);
                        UnityEditor.AssetDatabase.Refresh();

                        var config = JsonUtility.FromJson<RemoteConfig>(data);

                        if (config.ads_settings.maxsdk_enabled != 1
                            || String.IsNullOrEmpty(config.ads_settings.maxsdk_interstitial_id)
                            || String.IsNullOrEmpty(config.ads_settings.maxsdk_rewarded_id)
                            || String.IsNullOrEmpty(config.ads_settings.maxsdk_key))
                        {
                            result = "MaxMediation is not configured, please connect with SayGames support team.";
                        }

                    }
                    catch (System.Exception e)
                    {
                        result = errorTemplate + "Error in saving. " + e;
                    }
                }
                else
                {
                    result = errorTemplate + "Error in downloading. " + sayKitWebRequest.ErrorMessage;
                }
            }
            return result;
        }
    }

    class CheckRemoteConfigs : IPreBuildTask
    {

        [Serializable]
        public class SayKitRemoteConfiguration
        {
            public string Name;
            public string Data;
        }

        [Serializable]
        public class SayKitRemoteData
        {
            public int Version;
            public string Error;
            public string Platform;
            public List<SayKitRemoteConfiguration> Configuration;

            public string Attribution;
            public string AttributionToken;


            public string[] Alerts;
            public string[] Messages;
        }

        public string Title() { return "Checking remote configurations"; }
        public string Run()
        {
            return DownloadConfig();
        }


        public string GetRemoteURL()
        {
            string url = "https://api.launcher.saygames.io/saykit/configure?";

#if UNITY_IOS
            url += "app_key=" + SayKitApp.APP_KEY_IOS;
            url += "&app_secret=" + SayKitApp.APP_SECRET_IOS;
            url += "&saykit_platform=" + "ios";
#elif UNITY_ANDROID
            url += "app_key=" + SayKitApp.APP_KEY_ANDROID;
            url += "&app_secret=" + SayKitApp.APP_SECRET_ANDROID;
            url += "&saykit_platform=" + "android";
#endif

            url += "&app_version=" + Application.version;
            url += "&saykit_version=" + SayKit.GetVersion;

            url += "&place=" + buildPlace;
            url += "&device_id=" + SystemInfo.deviceUniqueIdentifier;

            return url;
        }

        
        public string DownloadConfig()
        {
            string url = GetRemoteURL();

            SayKitWebRequest sayKitWebRequest = new SayKitWebRequest(url);
            sayKitWebRequest.SendAndWait(10);

            try
            {
                if (sayKitWebRequest.IsDone && string.IsNullOrEmpty(sayKitWebRequest.ErrorMessage))
                {
                    if (sayKitWebRequest.Text.Length > 0 && sayKitWebRequest.Text[0] == '{')
                    {
                        var data = sayKitWebRequest.Text;

                        string path = "Assets/Resources";
                        string sayKitAttributionSettingsFilePath = path + "/saykit_attribution_settings.json";


                        var attributionData = new AttributionData();

                        var config = JsonUtility.FromJson<SayKitRemoteData>(data);



                        if (config.Error.Length == 0)
                        {
                            if (config.Alerts?.Length > 0)
                            {
                                for (int i = 0; i < config.Alerts.Length; i++)
                                {
                                    EditorUtility.DisplayDialog("SayKit Alert", config.Alerts[i], "OK");
                                    Debug.LogError("SayKit: " + config.Alerts[i]);
                                }

                            }


                            if (config.Messages?.Length > 0)
                            {
                                for (int i = 0; i < config.Messages.Length; i++)
                                {
                                    EditorUtility.DisplayDialog("SayKit Message", config.Messages[i], "OK");
                                    Debug.Log("SayKit: " + config.Messages[i]);
                                }
                            }

                            bool isFirebaseSettingsFinded = false;
                            for (int i = 0; i < config.Configuration.Count; i++)
                            {
                                if (config.Configuration[i].Name == "firebase")
                                {
                                    isFirebaseSettingsFinded = true;
#if UNITY_IOS
                                    string googlePlistPath = Application.dataPath + "/Plugins/iOS/GoogleService-Info.plist";
                                    DirectoryInfo dirInfo = new DirectoryInfo(googlePlistPath);
                                    string destinationPath = dirInfo.FullName;

                                    File.WriteAllText(destinationPath, config.Configuration[i].Data);
#elif UNITY_ANDROID
                                    string googleJsonPath = Application.dataPath + "/Plugins/Android/google-services.json";
                                    DirectoryInfo dirInfo = new DirectoryInfo(googleJsonPath);
                                    string destinationPath = dirInfo.FullName;

                                    File.WriteAllText(destinationPath, config.Configuration[i].Data);
#endif
                                }
                                else if (config.Configuration[i].Name == "facebook_app_id")
                                {
                                    ApplicationSettings.facebook_app_id = config.Configuration[i].Data;
                                }
                                else if (config.Configuration[i].Name == "facebook_app_name")
                                {
                                    ApplicationSettings.facebook_app_name = System.Security.SecurityElement.Escape(config.Configuration[i].Data);
                                }
                                else if (config.Configuration[i].Name == "admob_app_id")
                                {
                                    ApplicationSettings.admob_app_id = config.Configuration[i].Data;
                                }
                            }


                            attributionData.Attribution = config.Attribution;
                            attributionData.AttributionToken = config.AttributionToken;

                            if (attributionData.AttributionToken == null
                                || attributionData.AttributionToken == "")
                            {
                                return "Attribution is not configurated. Please, check your internet connection or connect to SayGames support team.";
                            }
                            else
                            {
                                var attributionJSON = JsonUtility.ToJson(attributionData);
                                File.WriteAllText(sayKitAttributionSettingsFilePath, attributionJSON);

                                AssetDatabase.ImportAsset(sayKitAttributionSettingsFilePath);
                            }

                            if (isFirebaseSettingsFinded)
                            {
                                return "";
                            }
                            else
                            {
                                Debug.LogError("Error when loading google settings. Server configuration doesn't contain google settings.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Error when loading google settings: " + config.Error);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error when loading google settings, content data is not correct: " + sayKitWebRequest.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when loading google settings: " + ex.Message);
            }

            return "Google settings wasn't loaded from server. Please, check the logs for more information.";
        }
    }

    class CreateDependentFolders : IPreBuildTask
    {

        public string Title() { return "Create dependent folders."; }
        public string Run()
        {

            string pluginPath = "Assets/Plugins";
            if (!System.IO.Directory.Exists(pluginPath))
            {
                System.IO.Directory.CreateDirectory(pluginPath);
            }

            string iOSPath = "Assets/Plugins/iOS";
            if (!System.IO.Directory.Exists(iOSPath))
            {
                System.IO.Directory.CreateDirectory(iOSPath);
            }

            string androidPath = "Assets/Plugins/Android";
            if (!System.IO.Directory.Exists(androidPath))
            {
                System.IO.Directory.CreateDirectory(androidPath);
            }


            string sayKitPath = "Assets/Plugins/Android/saykit";
            if (!System.IO.Directory.Exists(sayKitPath))
            {
                System.IO.Directory.CreateDirectory(sayKitPath);
            }


            string resPath = "Assets/Plugins/Android/saykit/res";
            if (!System.IO.Directory.Exists(resPath))
            {
                System.IO.Directory.CreateDirectory(resPath);
            }

            string maxPath = "Assets/Plugins/Android/saykit/MaxMediation";
            if (!System.IO.Directory.Exists(maxPath))
            {
                System.IO.Directory.CreateDirectory(maxPath);
            }

            string valuesPath = "Assets/Plugins/Android/saykit/res/values";
            if (!System.IO.Directory.Exists(valuesPath))
            {
                System.IO.Directory.CreateDirectory(valuesPath);
            }
           

            string xmlPath = "Assets/Plugins/Android/saykit/res/xml";
            if (!System.IO.Directory.Exists(xmlPath))
            {
                System.IO.Directory.CreateDirectory(xmlPath);
            }

            return "";
        }
    }


    class CheckScriptingSymbols : IPreBuildTask
    {

        public string Title() { return "Checking Scripting Symbols"; }
        public string Run()
        {

            var errors = new List<string>();

            bool purchasesSymbol = false;
            bool notificationsSymbol = false;

#if SAYKIT_PURCHASING
            purchasesSymbol = true;
#endif

#if SAYKIT_NOTIFICATIONS
            notificationsSymbol = true;
#endif

            if (SayKitApp.purchasesEnabled && !purchasesSymbol)
            {
                errors.Add("Purchases is enabled, but SAYKIT_PURCHASING symbol is not defined.\nRefer SayKit README for details.");
            }

            if (SayKitApp.notificationsEnabled && !notificationsSymbol)
            {
                errors.Add("Notifications is enabled, but SAYKIT_NOTIFICATIONS symbol is not defined.\nRefer SayKit README for details.");
            }

            return System.String.Join("\n\n", errors);
        }
    }


    class ExternalDependencyManagerConfig : IPreBuildTask
    {

        public string Title() { return "Configurate ExternalDependencyManager"; }
        public string Run()
        {
            string settingsPath = "Assets/SayKit/Internal/Plugins/Settings/";


#if SAYKIT_AUTOBUILD
            string gvhProjectSettingsPath = Path.Combine(settingsPath, "GvhProjectSettingsBuildMachine.xml");
#else
            string gvhProjectSettingsPath = Path.Combine(settingsPath, "GvhProjectSettings.xml");
#endif

            string destinationPath = "ProjectSettings/";
            string gvhProjectSettingsDestinationPath = Path.Combine(destinationPath, "GvhProjectSettings.xml");

            return CheckFile(gvhProjectSettingsPath, gvhProjectSettingsDestinationPath, "GvhProjectSettings.xml");
        }

        public string CheckFile(string baseFile, string targetFile, string fileName)
        {
            string result = "";

            if (!System.IO.File.Exists(targetFile))
            {
                File.Copy(baseFile, targetFile, true);
                result = "External Dependency Manager config is successfully updated. To complete the setup you need to reload Unity.";
            }
            else
            {
                if (!CompareFiles(baseFile, targetFile))
                {
                    result = fileName + " file doesn't contain depended lines. " +
                                "Please, check a " + targetFile + " file." +
                                " It needs to contain all data from a " + baseFile + " file."
                                + "\nYou can delete " + targetFile + " and it will be generated correctly.";


#if !SAYKIT_AUTOFIX_DISABLE
                    File.Copy(baseFile, targetFile, true);                
                    result = "External Dependency Manager config is successfully updated. To complete the setup you need to reload Unity.";
#endif
                }
                else
                {
                    result = "";
                }
            }
            
            return result;
        }
    }

    class CheckBuildSettings : IPreBuildTask
    {

        public string Title() { return "Checking build settings"; }
        public string Run()
        {
#if UNITY_IOS

#elif UNITY_ANDROID
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                return "Set IL2CPP as Scripting Backend\n\nGo to Player Settings -> Other Settings -> Configuration";
            }
#endif

            return "";
        }

    }

    class CheckMultiDexFabricApplicationSettings : IPreBuildTask
    {

        public string Title() { return "Checking MultiDexFabricApplication settings"; }
        public string Run()
        {

#if UNITY_ANDROID
            string path = "Assets/Plugins/Android/";
            string manifestPath = Path.Combine(path, "AndroidManifest.xml");

            if (System.IO.File.Exists(manifestPath))
            {
                string str = File.ReadAllText(manifestPath);
                if (str.Contains("MultiDexFabricApplication"))
                {
                    return "AndroidManifest.xml contains MultiDexFabricApplication settings. Please, delete android:name=\"io.fabric.unity.android.MultiDexFabricApplication\" line from Assets/Plugins/Android/AndroidManifest.xml file.";
                }
            }
#endif

            return "";
        }

    }



    class ConfigurateGenerateFiles : IPreBuildTask
    {
        private string _settingsPath = "Assets/SayKit/Internal/Plugins/Settings/";

        public string Title() { return "Configurate gradle and manifest files."; }
        public string Run()
        {

#if UNITY_ANDROID




#if UNITY_2020_2_OR_NEWER
            var gradleResult = CheckGradleFile("mainTemplate2020.gradle", "mainTemplate.gradle");
            var baseGradleResult = CheckGradleFile("baseProjectTemplate2020.gradle", "baseProjectTemplate.gradle");
            var gradleTemplateResult = CheckGradleFile("gradleTemplate2020.properties", "gradleTemplate.properties");
            var launcherTemplate = CheckGradleFile("launcherTemplate2020.gradle", "launcherTemplate.gradle");


            if (baseGradleResult.Length > 0)
            {
                return baseGradleResult;
            }

            if (gradleTemplateResult.Length > 0)
            {
                return gradleTemplateResult;
            }

            if (launcherTemplate.Length > 0)
            {
                return launcherTemplate;
            }
#elif UNITY_2019_3_OR_NEWER
            var gradleResult = CheckGradleFile("mainTemplate2019.gradle", "mainTemplate.gradle");
            var baseGradleResult = CheckGradleFile("baseProjectTemplate2019.gradle", "baseProjectTemplate.gradle");
            var gradleTemplateResult = CheckGradleFile("gradleTemplate2019.properties", "gradleTemplate.properties");
            var launcherTemplate = CheckGradleFile("launcherTemplate2019.gradle", "launcherTemplate.gradle");
            

            if (baseGradleResult.Length > 0)
            {
                return baseGradleResult;
            }

            if (gradleTemplateResult.Length > 0)
            {
                return gradleTemplateResult;
            }

            if (launcherTemplate.Length > 0)
            {
                return launcherTemplate;
            }
#else
            var gradleResult = CheckGradleFile("mainTemplate.gradle", "mainTemplate.gradle");
#endif

            var manifestResult = CheckManifestFile();
            var sayKitResult = CheckSayKitInitialize();
            
            var networkSecurityResult = CheckNetworkSecurityConfigFile();

            var maxResult = CheckMaxFile();
            var valuesSettingsResult = CheckValuesSettings();

            


            if (maxResult.Length > 0)
            {
                return maxResult;
            }
            if (gradleResult.Length > 0)
            {
                return gradleResult;
            }
            if (manifestResult.Length > 0)
            {
                return manifestResult;
            }
            if (sayKitResult.Length > 0)
            {
                return sayKitResult;
            }
            if (networkSecurityResult.Length > 0)
            {
                return networkSecurityResult;
            }
            if (valuesSettingsResult.Length > 0)
            {
                return valuesSettingsResult;
            }

#endif

            AssetDatabase.Refresh();

            return "";
        }

        public string CheckGradleFile(string gradleName, string gradleDestinationName)
        {
            string destinationPath = "Assets/Plugins/Android/";

            string gradlePath = Path.Combine(_settingsPath, gradleName);

            string gradleDestinationPath = Path.Combine(destinationPath, gradleDestinationName);

            return CheckFile(gradlePath, gradleDestinationPath, gradleDestinationName);
        }

        public string CheckManifestFile()
        {
            string destinationPath = "Assets/Plugins/Android/";


#if UNITY_2020_2_OR_NEWER
            string manifestPath = Path.Combine(_settingsPath, "AndroidManifest2020.xml");
#else
            string manifestPath = Path.Combine(_settingsPath, "AndroidManifest.xml");
#endif

            string manifestDestinationPath = Path.Combine(destinationPath, "AndroidManifest.xml");

            return CheckFile(manifestPath, manifestDestinationPath, "AndroidManifest.xml");
        }

        public string CheckMaxFile()
        {
            string destinationPath = "Assets/Plugins/Android/saykit/MaxMediation/";

            string manifestPath = Path.Combine(_settingsPath + "saykit/MaxMediation/", "AndroidManifest.xml");
            string manifestDestinationPath = Path.Combine(destinationPath, "AndroidManifest.xml");

            string propertiesPath = Path.Combine(_settingsPath + "saykit/MaxMediation/", "project.properties");
            string propertiesDestinationPath = Path.Combine(destinationPath, "project.properties");


            File.Copy(manifestPath, manifestDestinationPath, true);
            File.Copy(propertiesPath, propertiesDestinationPath, true);


            return "";
        }


        public string CheckSayKitInitialize()
        {
            var directoryPath = "Assets/Plugins/Android/saykit";

            CheckDirectory(directoryPath);


            string projectPropertiesPath = Path.Combine(_settingsPath + "saykit/", "project.properties");
            string projectPropertiesDestinationPath = Path.Combine(directoryPath + "/", "project.properties");

            if (CheckFile(projectPropertiesPath, projectPropertiesDestinationPath, "project.properties").Length > 0)
            {
                var lines = new List<string>
                    {
                        "android.library=true"
                    };

                File.AppendAllLines(projectPropertiesDestinationPath, lines);
            }


            string manifestPath = Path.Combine(_settingsPath + "saykit/", "AndroidManifest.xml");
            string manifestDestinationPath = Path.Combine(directoryPath + "/", "AndroidManifest.xml");

            return CheckFile(manifestPath, manifestDestinationPath, "AndroidManifest.xml");
        }

        public string CheckNetworkSecurityConfigFile()
        {
            var resDirectoryPath = "Assets/Plugins/Android/saykit/res";

            CheckDirectory(resDirectoryPath);

            var xmlDirectoryPath = "Assets/Plugins/Android/saykit/res/xml";

            CheckDirectory(xmlDirectoryPath);


            string networkSecurityConfigFilePath = Path.Combine(_settingsPath + "saykit/res/xml/", "network_security_config.xml");
            string networkSecurityConfigDestinationPath = Path.Combine(xmlDirectoryPath + "/", "network_security_config.xml");

            return CheckFile(networkSecurityConfigFilePath, networkSecurityConfigDestinationPath, "network_security_config.xml");
        }


        public string CheckValuesSettings()
        {
            if (String.IsNullOrEmpty(ApplicationSettings.facebook_app_name)
                || String.IsNullOrEmpty(ApplicationSettings.facebook_app_id)
                || String.IsNullOrEmpty(ApplicationSettings.admob_app_id))
            {
                return "facebook_app_name, facebook_app_id or admob_app_id did't download from server. Please, check your internet connection or connect to SayGames support team.";
            }
            else
            {

                var stringsPath = "Assets/Plugins/Android/saykit/res/values/strings.xml";
                var lines = new List<string>
                    {
                        "<resources>",
                        "<string name=\"app_name\">" + ApplicationSettings.facebook_app_name + "</string>",
                        "<string name=\"facebook_app_id\">" + ApplicationSettings.facebook_app_id + "</string>",
                        "<string name=\"fb_login_protocol_scheme\">fb" + ApplicationSettings.facebook_app_id + "</string>",
                        "<string name=\"admob_app_id\">" + ApplicationSettings.admob_app_id + "</string>",
                        "</resources>"
                    };

                if (!System.IO.File.Exists(stringsPath))
                {
                    File.WriteAllLines(stringsPath, lines);
                }
                else
                {

#if !SAYKIT_AUTOFIX_DISABLE
                    File.WriteAllLines(stringsPath, lines);
#endif

                    string[] baseFileLines = File.ReadAllLines(stringsPath);

                    foreach (var item in baseFileLines)
                    {
                        if (item.Length > 0 && !lines.Any(t => t.Replace(" ", "") == item.Replace(" ", "")))
                        {
                            return "facebook_app_name, facebook_app_id or admob_app_id aren't configurated correctrly. Please, check a Assets/Plugins/Android/saykit/res/values/strings.xml file.";
                        }
                    }
                }


                var maxStringsPath = "Assets/Plugins/Android/saykit/MaxMediation/AndroidManifest.xml";
                var maxData = System.IO.File.ReadAllLines(maxStringsPath);
                for (int i = 0; i < maxData.Length; i++)
                {
                  
                    if (maxData[i].Contains("INSERT_YOUR_ADMOB_APP_ID_HERE"))
                    {
                        maxData[i] = maxData[i].Replace("INSERT_YOUR_ADMOB_APP_ID_HERE", ApplicationSettings.admob_app_id);
                    }
                }
                File.WriteAllLines(maxStringsPath, maxData);

            }

            return "";
        }


        public void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string CheckFile(string baseFile, string targetFile, string fileName)
        {
            if (!System.IO.File.Exists(targetFile))
            {
                File.Copy(baseFile, targetFile, true);
            }
            else
            {
#if !SAYKIT_AUTOFIX_DISABLE
                File.Copy(baseFile, targetFile, true);
#endif

                if (!CompareFiles(baseFile, targetFile))
                {
                    return fileName + " file doesn't contain depended lines. " +
                        "Please, check a " + targetFile + " file." +
                        " It needs to contain all data from a " + baseFile + " file."
                        + "\nYou can delete " + targetFile + " and it will be generated correctly.";
                }

            }

            return "";
        }

    }



    class CheckGoogleServicesSettings : IPreBuildTask
    {

        public string Title() { return "Checking google services settings"; }
        public string Run()
        {

#if UNITY_IOS
            string path = "Assets/Plugins/iOS/";
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.plist");

            bool isFileExist = false;

            foreach (FileInfo file in Files)
            {
                if(file.Name == "GoogleService-Info.plist")
                {
                    isFileExist = true;
                }
            }

            if(!isFileExist)
            {
                return "File GoogleService-Info.plist is not found. You need to download it from Firebase console.";
            }


            float.TryParse(PlayerSettings.iOS.targetOSVersionString, out float targetOSVersion);
            if (targetOSVersion < 10)
            {
                return "Minimum supported iOS version is 10.0. Please update target minimum iOS version in player settings.";
            }

#elif UNITY_ANDROID
            string path = "Assets/Plugins/Android/";
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.json");

            bool isFileExist = false;

            foreach (FileInfo file in Files)
            {
                if (file.Name == "google-services.json")
                {
                    isFileExist = true;
                }
            }

            if (!isFileExist)
            {
                return "File google-services.json is not found. You need to download it from Firebase console";
            }

#endif

            return "";
        }
    }

    public class AndroidLibObject
    {
        public AndroidLibObject(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name;
        public string Version;
        public bool IsConfigurated;
    }

    public class AndroidVersionList
    {

        public List<AndroidLibObject> GradleLibsList;
        public List<AndroidLibObject> GradleClasspathList;
        public List<AndroidLibObject> GradleFabricList;
        public List<AndroidLibObject> JarLibsList;
        public List<AndroidLibObject> AarLibsList;
        public List<string> SayMedFileLibsList;

        public AndroidVersionList()
        {
            /* Gradle ******* */

            GradleLibsList = new List<AndroidLibObject>
            {
                new AndroidLibObject("facebook-android-sdk", "9.1.1"),
                new AndroidLibObject("audience-network-sdk", "6.5.1"),

                new AndroidLibObject("play-services-ads", "20.3.0"),

                new AndroidLibObject("firebase-crashlytics", "18.2.1"),
                new AndroidLibObject("firebase-analytics", "19.0.1"),
                new AndroidLibObject("firebase-messaging", "22.0.0"),
            };

            GradleClasspathList = new List<AndroidLibObject>
            {
#if !UNITY_2019_3_OR_NEWER
                new AndroidLibObject("google-services", "4.3.3"),
                new AndroidLibObject("firebase-crashlytics-gradle", "2.1.0")
#endif
            };

            GradleFabricList = new List<AndroidLibObject>
            {
#if !UNITY_2019_3_OR_NEWER
                new AndroidLibObject("com.google.gms.google-services", "")
#endif
            };

            /* JAR ******* */

            JarLibsList = new List<AndroidLibObject>
            {
                new AndroidLibObject("tenjin", "1.9.3"),
            };

            /* AAR ******* */

            AarLibsList = new List<AndroidLibObject>
            {
                new AndroidLibObject("facebook-biddingkit", "0.4.1"),
            };

            SayMedFileLibsList = new List<string>
            {
                "gson",
            };
        }

    }


    class CheckAndroidSettings : IPreBuildTask
    {

        public string Title() { return "Checking gradle settings"; }

        public string CutGradleLibVersion(string str)
        {
            if (str.Contains("@aar"))
            {
                return CutGradleLibVersion(str, 5);
            }
            else
            {
                return CutGradleLibVersion(str, 1);
            }
        }

        private string CutGradleLibVersion(string str, int tailLength)
        {
            var splits = str.Split(':');
            var split = splits[splits.Length - 1];

            return split.Substring(0, split.Length - tailLength);
        }

        public string CutLibVersion(string str)
        {
            return CutLibVersion(str, 4);
        }

        public string CutGradleJarVersion(string str)
        {
            return CutLibVersion(str, 6);
        }

        public string CutGradleAarVersion(string str)
        {
            if (str.Contains("@aar"))
            {
                return CutGradleLibVersion(str, 5);
            }
            else
            {
                var splits = str.Split(',');
                var split = splits[0];
                return CutLibVersion(split, 1);
            }
        }

        private string CutLibVersion(string str, int tailLength)
        {
            var splits = str.Split('-');
            var split = splits[splits.Length - 1];

            return split.Substring(0, split.Length - tailLength);
        }

        public bool CheckVersionList(List<AndroidLibObject> versionList)
        {
            bool isAllLibsInitialized = true;
            foreach (var lib in versionList)
            {
                if (!lib.IsConfigurated)
                {
                    isAllLibsInitialized = false;
                    Debug.LogError("Error: " + " Library: " + lib.Name + " " + lib.Version + "  is not found.");
                }
            }

            return isAllLibsInitialized;
        }

        public void CheckLibsDirectory(List<AndroidLibObject> versionList, string type)
        {
            string path = "Assets/SayKit/Internal/Plugins/Android";

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*." + type);

            foreach (FileInfo file in Files)
            {
                foreach (var lib in versionList)
                {
                    if (file.Name.Contains(lib.Name))
                    {
                        var version = CutLibVersion(file.Name);
                        if (lib.Version == "" || lib.Version.Equals(version))
                        {
                            lib.IsConfigurated = true;
                        }
                    }
                }
            }
        }

        public void CheckGradleFile(AndroidVersionList versionList)
        {
            string path = "Assets/Plugins/Android/";
            string gradlePath = Path.Combine(path, "mainTemplate.gradle");

            if (System.IO.File.Exists(gradlePath))
            {
                string str = File.ReadAllText(gradlePath);
                string[] lines = File.ReadAllLines(gradlePath);

                foreach (var line in lines)
                {
                    if (line.Contains("implementation"))
                    {

                        foreach (var lib in versionList.GradleLibsList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                var version = CutGradleLibVersion(line);
                                if (lib.Version.Equals(version))
                                {
                                    lib.IsConfigurated = true;
                                }
                            }
                        }
                    }
                    else if (line.Contains("classpath"))
                    {
                        foreach (var lib in versionList.GradleClasspathList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                var version = CutGradleLibVersion(line);
                                if (lib.Version.Equals(version))
                                {
                                    lib.IsConfigurated = true;
                                }
                            }
                        }
                    }
                    else if (line.Contains("maven") || line.Contains("apply"))
                    {
                        foreach (var lib in versionList.GradleFabricList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                lib.IsConfigurated = true;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private bool CheckSayMediationCompatible(AndroidVersionList versionList)
        {
            string libsPath = "Assets/SayKit/Internal/Plugins/Android";
            string path = "Assets/SayKit/Internal/SayMed/Internal/Plugins/Android";
            string gradlePath = Path.Combine(path, "build_gradle.txt");
            bool ok = true;
            var fileLibs =
                Directory.EnumerateFiles(libsPath, "*.jar", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(libsPath, "*.aar", SearchOption.AllDirectories))
                .Select(Path.GetFileName)
                .Where(file =>
                    !file.ToLower().Contains("mopub")
                    && !file.StartsWith("converter-gson")
                    && versionList.SayMedFileLibsList.Any(file.Contains))
                .Select(file => new AndroidLibObject(versionList.SayMedFileLibsList.Find(file.Contains), CutLibVersion(file)))
                .ToList();

            if (File.Exists(gradlePath))
            {
                string[] lines = File.ReadAllLines(gradlePath);

                foreach (var line in lines)
                {
                    if (line.Contains("compileOnly"))
                    {
                        foreach (var lib in versionList.GradleLibsList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                ok &= CheckSayMediationLib(lib, CutGradleLibVersion(line));
                            }
                        }
                        foreach (var lib in versionList.JarLibsList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                ok &= CheckSayMediationLib(lib, CutGradleJarVersion(line));
                            }
                        }
                        foreach (var lib in versionList.AarLibsList)
                        {
                            if (line.Contains(lib.Name))
                            {
                                ok &= CheckSayMediationLib(lib, CutGradleAarVersion(line));
                            }
                        }
                        foreach (var lib in fileLibs)
                        {
                            if (line.Contains(lib.Name))
                            {
                                ok &= CheckSayMediationLib(lib, CutGradleLibVersion(line));
                            }
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

            return ok;
        }

        private bool CheckSayMediationLib(AndroidLibObject lib, string expectedVersion)
        {
            if (!lib.Version.Equals(expectedVersion))
            {
                Debug.LogError("Error: " + " SayMed dependency " + lib.Name + " " + expectedVersion
                               + " doesn't match version " + lib.Version + " in SayKit");
                return false;
            }
            return true;
        }


        public string CheckMinAPILevel()
        {
            string errorMessage = "";

            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel19)
            {
                errorMessage = "You have to update minimum API level to 19 \n(Build Settings -> Player Settings -> Other settings -> Identification). \nPlease see the Readme file for more information.";
            }

#if UNITY_2019_3_OR_NEWER

            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel21)
            {
                errorMessage = "You have to update minimum API level to 21 \n(Build Settings -> Player Settings -> Other settings -> Identification). \nPlease see the Readme file for more information.";
            }
#endif

            return errorMessage;
        }


        public string Run()
        {
            AndroidVersionList versionList = new AndroidVersionList();

            CheckGradleFile(versionList);
            CheckLibsDirectory(versionList.JarLibsList, "jar");
            CheckLibsDirectory(versionList.AarLibsList, "aar");

            if (!CheckVersionList(versionList.GradleLibsList)
                || !CheckVersionList(versionList.GradleClasspathList)
                || !CheckVersionList(versionList.GradleFabricList)
                || !CheckVersionList(versionList.JarLibsList)
                || !CheckVersionList(versionList.AarLibsList))
            {
                return "Android libraries is not configured correctly. Please check the logs for more information.";
            }

            //if (!CheckSayMediationCompatible(versionList))
            //{
            //    return "SayMed framework is not compatible with current version of SayKit. Please check the logs for more information.";
            //}

            return CheckMinAPILevel();
        }
    }

    public static void OnlyFixExternalDependencyManagerConfig()
    {
#if !SAYKIT_EDM_CHECK_DISABLE
        new ExternalDependencyManagerConfig().Run();
#endif
    }

    public void OnPreprocessBuild(BuildReport report, bool releaseCheck)
    {

        int tasksNumber = 3;

        dirtyFlag = false;

        var tasks = new List<IPreBuildTask>();

        tasks.Add(new CreateDependentFolders());

        // ExternalDependencyManager
#if !SAYKIT_EDM_CHECK_DISABLE
        tasks.Add(new ExternalDependencyManagerConfig());
#endif

        // Embed Config
#if UNITY_IOS
        tasks.Add(new EmbedConfig(SayKitApp.APP_KEY_IOS));

        tasks.Add(new CheckRemoteConfigs());
        tasks.Add(new ConfigurateGenerateFiles());
        tasks.Add(new CheckGoogleServicesSettings());
#elif UNITY_ANDROID

#if UNITY_2019_3_OR_NEWER
        CheckKyestoreFile();

        //unity 2019.3 export file bug
        // File.WriteAllText($"{report.summary.outputPath}/build.gradle.NEW", "");
#endif
        tasks.Add(new EmbedConfig(SayKitApp.APP_KEY_ANDROID));

        tasks.Add(new CheckRemoteConfigs());
        tasks.Add(new ConfigurateGenerateFiles());
        tasks.Add(new CheckGoogleServicesSettings());

        tasks.Add(new CheckAndroidSettings());
        tasks.Add(new CheckMultiDexFabricApplicationSettings());
#endif

        // Check Symbols
        tasks.Add(new CheckScriptingSymbols());

        // Check platform settings
        if (releaseCheck)
        {
            tasks.Add(new CheckBuildSettings());
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            EditorUtility.DisplayProgressBar(DIALOG_TITLE, tasks[i].Title(), (float)i / tasksNumber);
            this.Assert(tasks[i].Run());
        }

        if (Application.isBatchMode)
        {
            if (dirtyFlag)
            {
                EditorApplication.Exit(1);
            }
        }

        // We are done.
        EditorUtility.ClearProgressBar();
    }

    public void Assert(string result)
    {
        if (result.Length > 0)
        {
            dirtyFlag = true;
            Debug.LogError("SayKit: " + result);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(DIALOG_TITLE, result, "OK");
            }
        }
    }

    public void CheckKyestoreFile()
    {
        string applicationPath = Application.dataPath.Replace("/Assets", "");
        DirectoryInfo dirInfo = new DirectoryInfo(applicationPath);

        var applicationFiles = dirInfo.GetFiles();

        for (int i = 0; i < applicationFiles.Length; i++)
        {
            var fileName = applicationFiles[i].Name;
            if (fileName.Contains("keystore"))
            {
                PlayerSettings.Android.keystoreName = Path.GetFullPath(fileName);
                return;
            }
        }
    }

    public string CheckSayKitUiPrefab()
    {

        string result = "";

        // Check if SayKitUI prefab exists
        if (EditorSceneManager.sceneCountInBuildSettings > 0)
        {
            var scene = EditorSceneManager.GetSceneByBuildIndex(0);
            if (!scene.isLoaded)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(0);
                scene = EditorSceneManager.OpenScene(path);
            }

            bool sayKitUIFound = false;

            var rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].GetComponent<SayKitUI>() != null)
                {
                    sayKitUIFound = true;
                    break;
                }
            }

            if (!sayKitUIFound)
            {
                result = "Add SayKitUI prefab on the top of first loading scene (build index = 0)";
            }

        }

        return result;
    }


    public static bool CompareFiles(string baseFile, string targetFile)
    {
        int i = 0;
        string[] baseFileLines = File.ReadAllLines(baseFile);
        string[] targetFileLines = File.ReadAllLines(targetFile);

        foreach (var item in baseFileLines)
        {
            i++;

            var line = item.Replace(" ", "").Replace("\t", ""); ;
            if (line.Length > 0 && !targetFileLines.Any(t => t.Replace(" ", "").Replace("\t", "") == line))
            {
                Debug.Log("Cannot find |" + line + "| line number " + i + " in a " + targetFile + " file. Please, compare data from it with a " + baseFile + " file.");
                return false;
            }
        }

        return true;
    }



}
#endif