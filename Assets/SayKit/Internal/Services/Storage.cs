using System;
using System.Collections.Generic;

using UnityEngine;

namespace SayKitInternal
{
    [Serializable]
    class StorageData
    {
        public string idfv = "";
        public bool consetWasGranted = false;
        public bool rateAppPopupWasShowed = false;

        public Dictionary<string, int> adsGroupPeriodEnds = new Dictionary<string, int>();
        public Dictionary<string, int> adsGroupPeriodImpressions = new Dictionary<string, int>();

        public bool isPremium = false;
    }

    [Serializable]
    class Storage
    {
        private static readonly Lazy<Storage> _lazyInstance = new Lazy<Storage>(() => new Storage());
        public static Storage instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        private StorageData _storageData;

        private string path
        {
            get
            {
                return Application.persistentDataPath + "/saykit.json";
            }
        }


        public bool isPremium { get { return _storageData.isPremium; } set { _storageData.isPremium = value; } }

        public string idfv { get { return _storageData.idfv; } set { _storageData.idfv = value; } }
        public bool consetWasGranted { get { return _storageData.consetWasGranted; } set { _storageData.consetWasGranted = value; } }
        public bool rateAppPopupWasShowed { get { return _storageData.rateAppPopupWasShowed; } set { _storageData.rateAppPopupWasShowed = value; } }

        public Dictionary<string, int> adsGroupPeriodEnds
        {
            get { return _storageData.adsGroupPeriodEnds; }
            set { _storageData.adsGroupPeriodEnds = value; }
        }

        public Dictionary<string, int> adsGroupPeriodImpressions
        {
            get { return _storageData.adsGroupPeriodImpressions; }
            set { _storageData.adsGroupPeriodImpressions = value; }
        }



        private Storage()
        {
            _storageData = new StorageData();
            InitDataFromFile();
        }

        private void InitDataFromFile()
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    var data = JsonUtility.FromJson<StorageData>(json);

                    if (data != null)
                    {
                        _storageData = data;
                    }

                    SayKitDebug.Log("Loaded SayKit storage config " + json);
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("SayKit storage init exception: " + exc.Message);
            }
        }


        public void save()
        {
            string data = JsonUtility.ToJson(_storageData);
            try
            {
                SayKitDebug.Log("Saving SayKit storage config " + data + " to " + path);
                System.IO.File.WriteAllText(path, data);
            }
            catch (Exception exc)
            {
                Debug.LogError("SayKit storage save exception: " + exc.Message);
            }
        }

    }
}