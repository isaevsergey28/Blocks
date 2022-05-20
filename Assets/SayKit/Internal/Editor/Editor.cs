#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


namespace SayKitInternal {

    public class Editor
    {

        private static readonly string SAYKIT_PURCHASING_SYMBOL = "SAYKIT_PURCHASING";
        private static readonly string SAYKIT_CHINA_VERSION_SYMBOL = "SAYKIT_CHINA_VERSION";

        public const string DIALOG_TITLE = "SayKit Message";



        [MenuItem("SayKit/Check Release build")]
        private static void EmbedConfig()
        {   
            var pb = new SayKitPreBuild();
            SayKitPreBuild.buildPlace = "check_release_build";

            pb.OnPreprocessBuild(null, true);
            if (!pb.dirtyFlag) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, "Finished without errors!\n\nᕦ( ͡° ͜ʖ ͡°)ᕤ", "Awesome!");
            }
        }



        [MenuItem("SayKit/Application config/World config")]
        public static void WorldConfig()
        {
            AddSayKitPurchaseDefineIfNeeded(SAYKIT_PURCHASING_SYMBOL);
            DeleteSayKitPurchaseDefineIfNeeded(SAYKIT_CHINA_VERSION_SYMBOL);

            PlayerSettings.applicationIdentifier = SayKitApp.APP_BUNDLE_IOS;
            PlayerSettings.productName = SayKitApp.APP_NAME_IOS;


            Menu.SetChecked("SayKit/Application config/World config", true);
            Menu.SetChecked("SayKit/Application config/China config", false);

            EmbedConfig();
        }


        [MenuItem("SayKit/Application config/China config")]
        public static void ChinaConfig()
        {
            AddSayKitPurchaseDefineIfNeeded(SAYKIT_CHINA_VERSION_SYMBOL);
            DeleteSayKitPurchaseDefineIfNeeded(SAYKIT_PURCHASING_SYMBOL);

            PlayerSettings.applicationIdentifier = SayKitApp.APP_BUNDLE_CHINA_IOS;
            PlayerSettings.productName = SayKitApp.APP_NAME_CHINA_IOS;


            Menu.SetChecked("SayKit/Application config/World config", false);
            Menu.SetChecked("SayKit/Application config/China config", true);

            EmbedConfig();
        }



        private static void AddSayKitPurchaseDefineIfNeeded(string symbol)
        {

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (defines.Contains(symbol))
            {
                Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + symbol + "</b> <i>Scripting Define Symbol</i>.");
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + symbol));
            Debug.LogWarning("<b>" + symbol + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
        }

        private static void DeleteSayKitPurchaseDefineIfNeeded(string symbol)
        {

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            string newDefines = "";

            if (defines.Contains(symbol))
            {
                int startSymbol = defines.IndexOf(";" + symbol);
                if (startSymbol < 0)
                {
                    startSymbol = defines.IndexOf(symbol);
                }

                int endSymbol = startSymbol + symbol.Length + 1;

                if (endSymbol < defines.Length)
                {
                    newDefines = defines.Substring(0, startSymbol) + defines.Substring(endSymbol, defines.Length - endSymbol);
                }
                else
                {
                    newDefines = defines.Substring(0, startSymbol);
                }


                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                Debug.LogWarning("<b>" + symbol + "</b> deleted from <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
            }
            else
            {
                Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") does not contain <b>" + symbol + "</b> <i>Scripting Define Symbol</i>.");
            }
        }

    }
}
#endif