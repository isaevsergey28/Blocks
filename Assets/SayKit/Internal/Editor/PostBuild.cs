#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

using UnityEditor.iOS.Xcode;
using System.Text;
using SayKitInternal;
#if UNITY_2017_1_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif


public class SayKitPostBuild {

    [PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject) {
		
       if (target != BuildTarget.iOS)
            return;

       Debug.Log("SayKit: Updating Info.plist");

#if UNITY_2018_3_3
		var frameworksPath = "Frameworks/";
#else
		var frameworksPath = "Frameworks/SayKit/Internal/Plugins/iOS/";
#endif

        string plistPath = Path.Combine(pathToBuildProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        //Get Root
        PlistElementDict rootDict = plist.root;

        //Add Queries Schemes
        PlistElementArray LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
        LSApplicationQueriesSchemes.AddString("fb");
        LSApplicationQueriesSchemes.AddString("instagram");

        List<string> queriesSchemes = new List<string>();
        queriesSchemes.Add("fb412266819304521");
        queriesSchemes.Add("fb2326383180965488");
        for (int i = 17; i <= 60; i++)
        {
            queriesSchemes.Add("saygames" + i);
        }

        foreach (var scheme in queriesSchemes)
        {
            if (SayKitApp.PROMO_KEY_IOS != scheme)
            {
                LSApplicationQueriesSchemes.AddString(scheme);
            }
        }

        // Register URL Schemes
        List<string> urlSchemes = new List<string>();
        urlSchemes.Add("fb" + SayKitRemoteSettings.SharedInstance.facebook_app_id);
        if (SayKitApp.PROMO_KEY_IOS != "")
        {
            urlSchemes.Add(SayKitApp.PROMO_KEY_IOS);
        }


        PlistElementArray CFBundleURLTypes = rootDict.CreateArray("CFBundleURLTypes");

        foreach (var scheme in urlSchemes)
        {
            PlistElementDict dict = CFBundleURLTypes.AddDict();
            PlistElementArray array = new PlistElementArray();
            array.AddString(scheme);
            dict["CFBundleURLSchemes"] = array;
        }


        rootDict.SetString("FacebookAppID", SayKitRemoteSettings.SharedInstance.facebook_app_id);
        rootDict.SetString("FacebookDisplayName", SayKitRemoteSettings.SharedInstance.facebook_app_name);

        PlistElementDict NSAppTransportSecurity = rootDict.CreateDict("NSAppTransportSecurity");
        NSAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);




        // rootDict.SetString("AppLovinSdkKey", "fItoM-flplDx4lFkAVVIIZTAi-0S2hpi_gULe95RuJNz2dgD7BWPm-ZSzZw_GkhFOBb1KhTC_s92TrGa1n-nmX");
        
        rootDict.SetString("NSCameraUsageDescription", "This app does not use the camera.");
        rootDict.SetString("NSCalendarsUsageDescription", "This app does not use the calendar.");
        rootDict.SetString("NSPhotoLibraryUsageDescription", "This app does not use the photo library.");
        rootDict.SetString("NSMotionUsageDescription", "This app does not use the accelerometer.");
		rootDict.SetString("NSLocationAlwaysUsageDescription", "This app does not use the location.");
        rootDict.SetString("NSLocationWhenInUseUsageDescription", "This app does not use the location.");
        rootDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "This app does not use the location.");


#if SAYKIT_CHINA_VERSION
        rootDict.SetString("NSUserTrackingUsageDescription", "这只会用于服务更相关的广告");
#else
        rootDict.SetString("NSUserTrackingUsageDescription", "This will only be used to serve more relevant ads.");
#endif

        rootDict.SetBoolean("GADIsAdManagerApp", true);
		rootDict.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
		rootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", true);

        rootDict.SetString("GADApplicationIdentifier", SayKitRemoteSettings.SharedInstance.admob_app_id);

        // Remote Notifications
        if (SayKitApp.notificationsEnabled) {
			PlistElementArray UIBackgroundModes = rootDict.CreateArray("UIBackgroundModes");
			UIBackgroundModes.AddString ("remote-notification");
		}

        // Remove "UIApplicationExitsOnSuspend" flag.
        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (rootDict.values.ContainsKey(exitsOnSuspendKey))
        {
            rootDict.values.Remove(exitsOnSuspendKey);
        }

        InsertSkAdNetworkIds(rootDict);



        File.WriteAllText (plistPath, plist.WriteToString());

		// XCode project
		Debug.Log("SayKit: Updating Xcode project");


        var projPath = Path.Combine(pathToBuildProject, "Unity-iPhone.xcodeproj/project.pbxproj");
        var project = new PBXProject();
        var projectFile = File.ReadAllText(projPath);

        project.ReadFromString(projectFile);


#if UNITY_2020_1_OR_NEWER
        var projTarget = project.GetUnityFrameworkTargetGuid();
        var mainTarget = project.GetUnityMainTargetGuid();

        project.AddBuildProperty(projTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");
        project.AddBuildProperty(mainTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");

#elif UNITY_2019_3_OR_NEWER
        var projTarget = project.GetUnityFrameworkTargetGuid();
        var mainTarget = project.GetUnityMainTargetGuid();

        project.AddBuildProperty(projTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");
        project.AddBuildProperty(mainTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");

#else
        var projTarget = project.TargetGuidByName("Unity-iPhone");

        project.AddBuildProperty(projTarget, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");

#endif


        project.SetBuildProperty(
            projTarget, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");

        project.AddBuildProperty(projTarget, "OTHER_LDFLAGS", "-ObjC");
        project.AddBuildProperty(projTarget, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
        project.AddBuildProperty(projTarget, "CLANG_ENABLE_MODULES", "YES");
        project.AddBuildProperty(projTarget, "ENABLE_BITCODE", "NO");

        if (!projectFile.Contains("SWIFT_VERSION"))
        {
            project.AddBuildProperty(projTarget, "SWIFT_VERSION", "5.0");
        }


        // Required Frameworks
        var frameworks = new string[] {
            "Accelerate.framework",
			"AdSupport.framework",
			"AVFoundation.framework",
			"CoreGraphics.framework",
			"CoreLocation.framework",
			"CoreMedia.framework",
			"CoreTelephony.framework",
			"Foundation.framework",
			"MediaPlayer.framework",
			"MessageUI.framework",
			"QuartzCore.framework",
			"SafariServices.framework",
			"StoreKit.framework",
			"SystemConfiguration.framework",
			"UIKit.framework",
			"WebKit.framework",

			"MobileCoreServices.framework",
			"Photos.framework",
            "VideoToolbox.framework",

            "AppTrackingTransparency.framework"
        };

		foreach (string framework in frameworks) {
			if (!project.ContainsFramework(projTarget, framework)) {
				Debug.Log ("SayKit: Adding " + framework + " to Xcode project");
				project.AddFrameworkToProject (projTarget, framework, false);
			}
		}


#if UNITY_CLOUD_BUILD
        project.RemoveFrameworkFromProject(projTarget, "StoreKit.framework");
#endif

        // Adding saypromo to embedded
#if UNITY_2019_3_OR_NEWER
        var saypromoGuid = project.FindFileGuidByProjectPath(frameworksPath + "saypromo.framework");
        project.AddFileToEmbedFrameworks(mainTarget, saypromoGuid);

        var saypromoBundle = project.FindFileGuidByProjectPath(frameworksPath + "saypromo-resources.bundle");
        project.AddFileToBuild(mainTarget, saypromoBundle);


         var apsGuid = project.FindFileGuidByProjectPath(frameworksPath + "DTBiOSSDK.framework");
		 project.AddFileToEmbedFrameworks(mainTarget, apsGuid);

        project.AddBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
        project.AddBuildProperty(projTarget, "ENABLE_BITCODE", "NO");

#elif UNITY_2017_1_OR_NEWER
        var saypromoGuid = project.FindFileGuidByProjectPath(frameworksPath + "saypromo.framework");
		project.AddFileToEmbedFrameworks(projTarget, saypromoGuid);

        var apsGuid = project.FindFileGuidByProjectPath(frameworksPath + "DTBiOSSDK.framework");
        project.AddFileToEmbedFrameworks(projTarget, apsGuid);
#endif


#if UNITY_2019_3_OR_NEWER || UNITY_2019_4_OR_NEWER
        CommentRowsInUnityCleanupTrampoline(pathToBuildProject);
#endif



        // Required Libs
        var libs = new string[] {
			"libresolv.9.tbd",
			"libc++.tbd",
			"libz.tbd",
            "libbz2.tbd",

            "libz.dylib",
            "libsqlite3.dylib",
            "libxml2.dylib"
        };

		foreach (string lib in libs) {
        	Debug.Log ("SayKit: Adding " + lib + " to Xcode project");

			string libGuid = project.AddFile("usr/lib/" + lib, "Libraries/" + lib, PBXSourceTree.Sdk);
			project.AddFileToBuild(projTarget, libGuid);
        }



#if !SAYKIT_UPLOAD_SYMB_DISABLE
        // Crashlytics
        if (project.GetShellScriptBuildPhaseForTarget(projTarget, "Firebase Crashlytics", "/bin/sh", "\"${PROJECT_DIR}/firebase-run\" --debug") == null)
        {
            project.AddShellScriptBuildPhase(projTarget, "Firebase Crashlytics", "/bin/sh", "\"${PROJECT_DIR}/firebase-run\"  --debug");
        }

        if (project.GetShellScriptBuildPhaseForTarget(projTarget, "Firebase Crashlytics dSYMs", "/bin/sh", "\"$PROJECT_DIR/firebase_symbols.sh\"") == null)
        {
            project.AddShellScriptBuildPhase(projTarget, "Firebase Crashlytics dSYMs", "/bin/sh", "\"$PROJECT_DIR/firebase_symbols.sh\"");
        }

#if UNITY_2019_3_OR_NEWER
        if (project.GetShellScriptBuildPhaseForTarget(mainTarget, "Firebase Crashlytics dSYMs", "/bin/sh", "\"$PROJECT_DIR/firebase_symbols.sh\"") == null)
        {
            project.AddShellScriptBuildPhase(mainTarget, "Firebase Crashlytics dSYMs", "/bin/sh", "\"$PROJECT_DIR/firebase_symbols.sh\"");
        }
#endif

        string firebase_symbols_Path = Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/firebase_symbols.sh";
        string project_firebase_symbols_Path = Path.Combine(pathToBuildProject, "firebase_symbols.sh");
        File.Copy(firebase_symbols_Path, project_firebase_symbols_Path, true);

        string firebase_run_Path = Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/firebase-run";
        string project_firebase_run_Path = Path.Combine(pathToBuildProject, "firebase-run");
        File.Copy(firebase_run_Path, project_firebase_run_Path, true);

        string firebase_upload_symbols_Path = Application.dataPath + "/SayKit/Internal/Plugins/iOS/SayKit/firebase-upload-symbols";
        string project_firebase_upload_symbols_Path = Path.Combine(pathToBuildProject, "firebase-upload-symbols");
        File.Copy(firebase_upload_symbols_Path, project_firebase_upload_symbols_Path, true);
#endif



#if UNITY_2020_1_OR_NEWER

        var swiftCheck = "echo \"Start Unity Swift Bug script.\"  \n" +
            "if [ \"${CONFIGURATION}\" = \"Release\" ]; then \n" +
            "cd \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/Frameworks/UnityFramework.framework/\" \n" +
            "if [[ -d \"Frameworks\" ]]; then \n" +
            "rm -fr Frameworks \n" +
            "echo \"Remove Frameworks folder from UnityFramework.framework.\" \n" +
            "fi \n" +
            "fi \n";

        if (project.GetShellScriptBuildPhaseForTarget(mainTarget, "Unity Swift Bug", "/bin/sh", swiftCheck) == null)
        {
            project.AddShellScriptBuildPhase(mainTarget, "Unity Swift Bug", "/bin/sh", swiftCheck);
        }

#endif


        File.WriteAllText(projPath, project.WriteToString());


        CopyGooglePlistFile(target, pathToBuildProject);
        CheckXCodeProjectSettings(target, pathToBuildProject);

        RemoveMetaFiles(pathToBuildProject);
        RenameMRAIDSource(pathToBuildProject);
    }

	private static void InsertSkAdNetworkIds(PlistElementDict rootDict)
	{
		string idsPath = Application.dataPath + "/SayKit/SKAdNetworkItems.json";
		string jsonContent = File.ReadAllText(idsPath);
		Dictionary<string, object> json = JsonConverter.Deserialize(jsonContent) as Dictionary<string, object>;
		List<object> networks = json["networks"] as List<object>;
		HashSet<string> allSkIds = new HashSet<string>();
		foreach (Dictionary<string, object> network in networks.Cast<Dictionary<string, object>>())
		{
			List<object> ids = network["items"] as List<object>;
			allSkIds.UnionWith(ids.Cast<string>());
		}

		PlistElementArray array = rootDict.CreateArray("SKAdNetworkItems");
		foreach (string id in allSkIds)
		{
			PlistElementDict pair = array.AddDict();
			pair["SKAdNetworkIdentifier"] = new PlistElementString(id);
		}
	}

	private static void SetupSayMediation(PBXProject project, string projTarget)
	{
		string SayMed = "SayMed.framework";
		string SayMed2019_3 = "SayMed_2019_3.framework";
//#if UNITY_2019_3_OR_NEWER
//		string requiredFramework = SayMed2019_3;
//		string removeFramework = SayMed;
//#else
		string requiredFramework = SayMed;
		string removeFramework = SayMed2019_3;
//#endif
#if UNITY_2018_3_3
		var frameworksPath = "Frameworks/";
#else
		var frameworksPath = "Frameworks/SayKit/Internal/SayMed/Internal/Plugins/iOS/";
#endif
		
		project.RemoveFrameworkFromProject(projTarget, removeFramework);
		string fullPath = frameworksPath + removeFramework;
		var fileId = project.FindFileGuidByProjectPath(fullPath);
		if (fileId != null)
		{
			project.RemoveFileFromBuild(projTarget, fileId);
			project.RemoveFile(fileId);
		}
		
		if (!project.ContainsFileByProjectPath(frameworksPath + requiredFramework))
		{
			Debug.LogError("SayKit: NOT FOUND " + requiredFramework);
			EditorUtility.DisplayDialog(DIALOG_TITLE, requiredFramework + " not found.", "OK");
		}
	}

	public static void CheckXCodeProjectSettings(BuildTarget target, string pathToBuiltProject)
    {
        var xcodeProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj");
        var pbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");

        var xcodeProjectLines = File.ReadAllLines(pbxPath);
        var sb = new StringBuilder();
        var isNeedToAddValidArchs = false;

        foreach (var line in xcodeProjectLines)
        {
            if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
                line.Contains("GCC_ENABLE_CPP_EXCEPTIONS") ||
                line.Contains("CLANG_ENABLE_MODULES"))
            {
                var newLine = line.Replace("NO", "YES");
                sb.AppendLine(newLine);

                isNeedToAddValidArchs = true;
            }
            else
            {
                sb.AppendLine(line);
            }

            if (isNeedToAddValidArchs && line.Contains("USYM_UPLOAD_URL_SOURCE"))
            {
                isNeedToAddValidArchs = false;
                sb.AppendLine("VALID_ARCHS = \"arm64 armv7 armv7s\";");
            }
        }

        File.WriteAllText(pbxPath, sb.ToString());
    }

    public static void CopyGooglePlistFile(BuildTarget buildTarget, string pathToBuildProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
            PBXProject proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));

#if UNITY_2019_3_OR_NEWER
            var projTarget = proj.GetUnityFrameworkTargetGuid();
            var mainTarget = proj.GetUnityMainTargetGuid();
#else
            string projTarget = proj.TargetGuidByName("Unity-iPhone");
#endif

            string googlePlistPath = Application.dataPath + "/Plugins/iOS/GoogleService-Info.plist";
            string googlePlistProjectPath = Path.Combine(pathToBuildProject, "GoogleService-Info.plist");

            File.Copy(googlePlistPath, googlePlistProjectPath, true);

            proj.AddFileToBuild(projTarget, proj.AddFolderReference("GoogleService-Info.plist", "GoogleService-Info.plist"));
            
#if UNITY_2019_3_OR_NEWER            
            proj.AddFileToBuild(mainTarget, proj.AddFolderReference("GoogleService-Info.plist", "GoogleService-Info.plist"));
#endif

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }

	private static void CheckXcodeProject(PBXProject project)
	{

#if UNITY_2019_3_OR_NEWER
        string projTarget = project.GetUnityFrameworkTargetGuid();
#else
        string projTarget = project.TargetGuidByName("Unity-iPhone");
#endif

        bool dirtyFlag = false;
		int totalChecks = requiredFiles.Length;
		float currentCheck = 0;

		var filesPath = "Libraries/SayKit/Internal/Plugins/iOS/";
		foreach (string file in requiredFiles) {
			string fullPath = filesPath + file;
			EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Checking " + file, currentCheck/totalChecks); currentCheck++;
			if (!project.ContainsFileByProjectPath(fullPath)) {
				Debug.LogError("SayKit: NOT FOUND " + fullPath);
				dirtyFlag = true;
			}
		}

		// We are done.
        EditorUtility.ClearProgressBar();

		if (dirtyFlag) {
			EditorUtility.DisplayDialog(DIALOG_TITLE, "Found some problems in Xcode project.\n\nPlease check Unity log for details.", "OK");
		} else {
			Debug.Log("SayKit: Xcode Project seems to be OK");
		}

	}

	

	/*
	
	Const section
	
	*/

	private static readonly string DIALOG_TITLE = "SayKit Xcode Project";

	// Required adapters
	// Cmd: find . -type "file" | sed -e "s/\.\/\(.*\)$/\"\1\",/g" | sort
	private static readonly string[] requiredFiles = {
		"SayKit/SayKitBinding.m",
		"SayKit/SayKitEvent.h",
		"SayKit/SayKitEvent.mm",
		"SayKit/SayPromoBinding.m",

		"Tenjin/TenjinSDK.h",
		"Tenjin/TenjinUnityInterface.h",
		"Tenjin/TenjinUnityInterface.mm",
		"Tenjin/libTenjinSDK.a",

        "Analytics/Firebase.h",
        };


    private static void RemoveMetaFiles(string buildPath)
    {
        // Remove all the .meta files that Unity copies into the Xcode subdirectories.

#if UNITY_2018_3_3
            foreach (var subdir in new[] { "Frameworks/", "Libraries/" }) {
#else
        foreach (var subdir in new[] { "Frameworks/SayKit/Internal/Plugins/iOS", "Libraries/SayKit/Internal/Plugins/iOS" })
        {
#endif

            var path = Path.Combine(buildPath, subdir);
            var metaFiles = Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories);
            foreach (var metaFile in metaFiles)
            {
                File.Delete(metaFile);
            }
        }
    }

    private static void RenameMRAIDSource(string buildPath)
    {
        // Unity will try to compile anything with the ".js" extension. Since mraid.js is not intended
        // for Unity, it'd break the build. So we store the file with a masked extension and after the
        // build rename it to the correct one.

        var maskedFiles = Directory.GetFiles(
            buildPath, "*.prevent_unity_compilation", SearchOption.AllDirectories);
        foreach (var maskedFile in maskedFiles)
        {
            var unmaskedFile = maskedFile.Replace(".prevent_unity_compilation", "");
            File.Move(maskedFile, unmaskedFile);
        }
    }


#if UNITY_2019_3_OR_NEWER || UNITY_2019_4_OR_NEWER
    private static void CommentRowsInUnityCleanupTrampoline(string pathToBuildProject)
    {
        string filePath = Path.Combine(pathToBuildProject, "Classes/UnityAppController.mm");
        string data = File.ReadAllText(filePath);

        data = data.Replace("[_UnityAppController window].rootViewController = nil;", "//[_UnityAppController window].rootViewController = nil;");
        data = data.Replace("[[_UnityAppController unityView] removeFromSuperview];", "//[[_UnityAppController unityView] removeFromSuperview];");
        
        File.WriteAllText(filePath, data);
    }
#endif

}
#endif
