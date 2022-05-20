using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;


namespace MeowLab.Helpers.VersionIncrementor
{
    public class BuildVersionData : SerializedScriptableObject
    {
        [Space]
        [InfoBox("@\"Last build time: \" + BuildTime")]
        public string AppName;
        public string NamePattern = "APPNAME_[VERSION]_[BUNDLE]_[DATE]";
        public string TimePattern = "MM/dd/yyyy_H:mm";
        [Space]
        public int MajorVersion = 0;
        public int MinorVersion = 1;
        public int BuildVersion = 1;
        [Space]
        [OdinSerialize] [HideInInspector] public DateTime Time;

        public string CurrentVersion => $"{MajorVersion:0}.{MinorVersion:00}.{BuildVersion:000}";
        public string BuildTime => Time.ToString(TimePattern);
        public int BundleVersion => MajorVersion * 10000 + MinorVersion * 100 + BuildVersion;


        public string GetName()
        {
            var name = NamePattern;
            name = name.Replace("APPNAME", AppName);
            name = name.Replace("VERSION", CurrentVersion.ToString());
            name = name.Replace("BUNDLE", BundleVersion.ToString());
            name = name.Replace("DATE", BuildTime);
            return name;
        }
        
        public string GetNameWithNowTime()
        {
            var name = NamePattern;
            name = name.Replace("APPNAME", AppName);
            name = name.Replace("VERSION", CurrentVersion.ToString());
            name = name.Replace("BUNDLE", BundleVersion.ToString());
            name = name.Replace("DATE", DateTime.Now.ToString(TimePattern));
            return name;
        }


        [Button]
        private void SaveToProjectSettings()
        {
#if UNITY_EDITOR
            PlayerSettings.bundleVersion = $"{MajorVersion:0}.{MinorVersion:00}";
            PlayerSettings.Android.bundleVersionCode = BundleVersion;
#endif
        }


        public void IncrementVersion(int majorIncr, int minorIncr, int buildIncr)
        {
            MajorVersion += majorIncr;
            MinorVersion += minorIncr;
            BuildVersion += buildIncr;
        }


        public void IncreaseMajor()
        {
            MajorVersion++;
            MinorVersion = 0;
            BuildVersion = 0;
            SaveToProjectSettings();
        }


        public void IncreaseMinor()
        {
            MinorVersion++;
            BuildVersion = 0;
            SaveToProjectSettings();
        }


        public void IncreaseBuild()
        {
            BuildVersion++;
            SaveToProjectSettings();
        }
    }
}