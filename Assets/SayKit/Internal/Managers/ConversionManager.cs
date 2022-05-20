using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SayKitInternal
{

    [Serializable]
    public class ConversionData
    {
        public ConversionConfig config;
        public List<ConversionSKADValues> skadValues;
    }

    [Serializable]
    public class ConversionResponseData
    {
        public ConversionConfig config;
        public ConversionSKADValues[] skadValues;
    }

    [Serializable]
    public class ConversionConfig
    {
        public int enabled;
        public int timer;
    }

    [Serializable]
    public class ConversionSKADValues
    {
        public string id;
        public int value;
        public int kind;
    }


    public class ConversionManager
    {
        public static ConversionManager Instance { get; } = new ConversionManager();


        private readonly object _lockSaveObject = new object();
        private readonly object _lockRequestObject = new object();

        private readonly string _fileName = "saykit_conversion_log.txt";
        private readonly int _requestTimeout = 5;

        private int _routineTimer = 15;
        private bool _conversionEnabled = true;


        private readonly string _url = "https://live.saygames.io/live/ping";

        private ConversionData _conversionData;



        public void Init()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;
            if (!(System.IO.File.Exists(cachePath)))
            {
                System.IO.File.Create(cachePath);

                //default config
                _conversionData = new ConversionData();
                _conversionData.config = new ConversionConfig();
                _conversionData.config.timer = 15;
                _conversionData.config.enabled = 1;
                _conversionData.skadValues = new List<ConversionSKADValues>();

                SaveConversionData();
            }
            else
            {
                ReadConversionData();
            }
        }


        private String GetUrl(String kind)
        {
            string url = _url + "?idfa=" + SayKit.runtimeInfo.idfa
                + "&idfv=" + SayKit.runtimeInfo.idfv
                + "&appKey=" + SayKit.config.appKey
                + "&version=" + SayKit.runtimeInfo.version
                + "&ts=" + Utils.currentTimestamp
                + "&installTs=" + ImpressionLogManager.Instance.ImpressionData.FirstStartTimestamp
                + "&saykit=" + SayKit.GetVersion
                + "&kind=" + kind;

            return url;
        }


        public void RunRequestConversionData(String kind)
        {
            SayKit.GetInstance().StartCoroutine(Instance.RequestConversionData(kind));
        }

        private IEnumerator RequestConversionData(String kind)
        {
            // SayKit.trackEvent("d_conversion_start", "[ kind = " + kind + ", totalCount = " + _conversionData.skadValues.Count + " ]");

            lock (_lockRequestObject)
            {
                string url = GetUrl(kind);

                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    webRequest.timeout = _requestTimeout;
                    yield return webRequest.SendWebRequest();

                    if (webRequest.isNetworkError)
                    {
                        SayKitDebug.Log("RequestConversionData: " + webRequest.error);
                    }
                    else
                    {
                        try
                        {
                            ConversionResponseData conversionData = JsonUtility.FromJson<ConversionResponseData>(webRequest.downloadHandler.text);

                            AnalyzeConversionData(conversionData);
                        }
                        catch (Exception exp)
                        {
                            SayKitDebug.Log("RequestConversionData error: " + exp.Message);
                        }
                    }
                }
            }

            // SayKit.trackEvent("d_conversion_end", "[ kind = " + kind + ", totalCount = " + _conversionData.skadValues.Count + " ]");
        }


        public IEnumerator startConversionRoutine()
        {
            InitConfigData();

            while (_conversionEnabled)
            {
                yield return RequestConversionData("timer");

                InitConfigData();

                yield return new WaitForSecondsRealtime(_routineTimer);
            }
        }


        private void AnalyzeConversionData(ConversionResponseData conversionData)
        {
            bool needToStartRoutine = false;

            if (conversionData != null)
            {
                if (conversionData.config != null)
                {
                    if (conversionData.config.enabled == 1 && _conversionData.config.enabled == 0)
                    {
                        needToStartRoutine = true;
                    }

                    _conversionData.config.enabled = conversionData.config.enabled;
                    _conversionData.config.timer = conversionData.config.timer;

                    _configDataInitialized = false;
                }

                if (conversionData.skadValues?.Length > 0)
                {
                    for (int i = 0; i < conversionData.skadValues.Length; i++)
                    {
                        ConversionSKADValues value = _conversionData.skadValues.FirstOrDefault(t => t.id.Equals(conversionData.skadValues[i].id));
                        if (value == null)
                        {
                            ConversionSKADValues newValue = new ConversionSKADValues
                            {
                                id = conversionData.skadValues[i].id,
                                value = conversionData.skadValues[i].value,
                                kind = conversionData.skadValues[i].kind
                            };

                            _conversionData.skadValues.Add(newValue);

                            SayKitBridje.Instance.updateConversionValue(newValue.value, newValue.kind, newValue.id);

                        }
                    }

                    SaveConversionData();
                }
            }

            if (needToStartRoutine)
            {
                SayKit.GetInstance().StartCoroutine(Instance.startConversionRoutine());
            }
        }


        private bool _configDataInitialized = false;
        private void InitConfigData()
        {
            if (!_configDataInitialized)
            {
                _configDataInitialized = true;

                _conversionEnabled = _conversionData.config.enabled == 1;
                _routineTimer = _conversionData.config.timer;
            }
        }

        private void SaveConversionData()
        {

            try
            {
                lock (_lockSaveObject)
                {
                    string cachePath = Application.persistentDataPath + "/" + _fileName;
                    var jsonData = JsonUtility.ToJson(_conversionData);

                    System.IO.File.WriteAllText(cachePath, jsonData);
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }
        }


        public void ReadConversionData()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;

            try
            {
                lock (_lockSaveObject)
                {
                    var jsonData = System.IO.File.ReadAllText(cachePath);

                    SayKitDebug.Log("ConversionManager: " + jsonData);

                    _conversionData = JsonUtility.FromJson<ConversionData>(jsonData);
                    if (_conversionData == null)
                    {
                        _conversionData = new ConversionData();
                        _conversionData.config = new ConversionConfig();
                        _conversionData.config.timer = 15;
                        _conversionData.config.enabled = 0;
                        _conversionData.skadValues = new List<ConversionSKADValues>();
                    }
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log("ReadAllImpressions error: " + exp.Message);
            }
        }
    }

}