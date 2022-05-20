using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace MeowLab.Helpers.VersionIncrementor.Editor
{
    public class VersionIncrementor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder { get; }


        public void OnPreprocessBuild(BuildReport report)
        {
            GetData().Time = DateTime.Now;
        }


        public void OnPostprocessBuild(BuildReport report)
        {
            IncreaseBuild();
        }


        public static BuildVersionData GetData()
        {
            var path = Path.Combine("Assets", "Resources", "VersionData.asset");
            var data = AssetDatabase.LoadAssetAtPath<BuildVersionData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<BuildVersionData>();
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = data;
            }

            return data;
        }


        [MenuItem("Build/Increase Major Version")]
        private static void IncreaseMajor()
        {
            var data = GetData();
            data.IncreaseMajor();
            EditorUtility.SetDirty(data);
        }


        [MenuItem("Build/Increase Minor Version")]
        private static void IncreaseMinor()
        {
            var data = GetData();
            data.IncreaseMinor();
            EditorUtility.SetDirty(data);
        }


        [MenuItem("Build/Increase Build")]
        private static void IncreaseBuild()
        {
            var data = GetData();
            data.IncreaseBuild();
            EditorUtility.SetDirty(data);
        }


        [MenuItem("Build/Copy Name")]
        private static void CopyName()
        {
            var data = GetData();
            var name = data.GetNameWithNowTime();
            var textEditor = new TextEditor();
            textEditor.text = name;
            textEditor.SelectAll();
            textEditor.Copy();
            Debug.Log($"{name} copied!");
        }
    }
}