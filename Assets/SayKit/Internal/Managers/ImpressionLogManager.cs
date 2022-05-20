using UnityEngine;

using System;
using System.Collections.Generic;
using System.Collections;

namespace SayKitInternal
{
    [Serializable]
    public class ImpressionObject
    {
        public ImpressionObject(string type, int timestamp)
        {
            Type = type;
            Timestamp = timestamp;
        }

        public string Type;
        public int Timestamp;
        public float CPM;
    }

    [Serializable]
    public class ImpressionData
    {
        public int FirstStartTimestamp;
        public List<ImpressionObject> Data = new List<ImpressionObject>();
        public List<string> AnalyzedEvents = new List<string>();
    }

    [Serializable]
    public class MoPubImpressionObject
    {
        public float cpm;
        public string appId;
        public string placeId;
    }

    public class ImpressionLogManager
    {
        private static Lazy<ImpressionLogManager> _lazyInstance = new Lazy<ImpressionLogManager>(() => new ImpressionLogManager());
        public static ImpressionLogManager Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }


        private readonly object _lockObject = new object();
        private readonly string _fileName = "saykit_impression_log.txt";


        public ImpressionData ImpressionData = new ImpressionData();


        public void Init()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;
            if (!(System.IO.File.Exists(cachePath)))
            {
                System.IO.File.Create(cachePath);
                if (ImpressionData == null) { ImpressionData = new ImpressionData(); }

                ImpressionData.FirstStartTimestamp = Utils.currentTimestamp;

                SaveImpressionData();
            }
            else
            {
                ReadAllImpressions();
            }
        }


        public void CheckImpressionStates()
        {

            ReadAllImpressions();
            try
            {
                var events = RemoteConfigManager.config.ads_settings.ping_events;


                if (events != null)
                {
                    var eventsArray = events.Split(',');
                    var states = GenerateStates();

                    foreach (var state in states)
                    {
                        bool exist = Array.Exists<string>(eventsArray, (obj) => obj == state);
                        if (exist)
                        {
                            if (!ImpressionData.AnalyzedEvents.Exists((obj) => obj == state))
                            {
                                //send message to our server
                                AnalyticsEvent.trackFirebaseEvent(state, "");
                                AnalyticsEvent.trackFacebookEvent(state, "");
                                AnalyticsEvent.trackEvent(state);

                                ImpressionData.AnalyzedEvents.Add(state);
                            }
                        }
                    }

                    SaveImpressionData();
                }
                else
                {
                    SayKitDebug.Log("ImpressionLogManager. Server events not found");
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }

        }




        private List<string> GenerateStates()
        {
            List<string> currentSates = new List<string>();

            int count = 0;
            float valueSum = 0;

            int maxTimestamp = 0;
            int minTimestamp = 0;

            foreach (var item in ImpressionData.Data)
            {
                if (item.Type == "r")
                {
                    count += 2;
                }
                else
                {
                    count += 1;
                }

                valueSum += item.CPM;

                if (maxTimestamp < item.Timestamp)
                {
                    maxTimestamp = item.Timestamp;
                }

                if (minTimestamp > item.Timestamp || minTimestamp == 0)
                {
                    minTimestamp = item.Timestamp;
                }
            }

            if (ImpressionData.FirstStartTimestamp == 0)
            {
                ImpressionData.FirstStartTimestamp = minTimestamp - 1;
            }

            var timespan = ((double)(maxTimestamp - ImpressionData.FirstStartTimestamp)) / 3600 / 24;
            var fullDay = (int)Math.Truncate(timespan);
            var fractDay = timespan - fullDay;

            if (fractDay > 0)
            {
                fullDay++;
            }


            for (int i = 1; i <= count; i++)
            {
                var imp = "imp_" + "d" + fullDay + "_" + i;
                currentSates.Add(imp);
            }

            var valueSumInt = (int)Math.Truncate((double)valueSum * 100) ;
            for (int i = 1; i <= valueSumInt; i++)
            {
                var ltv = "ltv_" + "d" + fullDay + "_" + i;
                currentSates.Add(ltv);
            }

            return currentSates;
        }




        public void RunSavingImpression(ImpressionObject impressionObject, float cpm)
        {
            SayKit.GetInstance().StartCoroutine(SaveNewImpression(impressionObject, cpm));
        }

        public IEnumerator SaveNewImpression(ImpressionObject impressionObject, float cpm)
        {
            try
            {
                // SayKit.trackEvent("d_impression_start", "[ type = " +impressionObject.Type + ", cpm = " + cpm + ", totalCount = " +ImpressionData.Data.Count + " ]" );

                if (cpm > 0)
                {
                    impressionObject.CPM = cpm;

                    AnalyticsEvent.trackFirebaseEventWithValue("ads_earnings", cpm);

                    Dictionary<string, object> facebookParams = new Dictionary<string, object>
                        {
                            { "extra", "ads_earnings" },
                            { "ads_earnings", cpm}
                        };

                    AnalyticsEvent.trackFullFacebookEvent("Donate", cpm, facebookParams);
                }


                var timespan = ((double)(Utils.currentTimestamp - ImpressionData.FirstStartTimestamp)) / 3600 / 24;
                var fullDay = (int)Math.Truncate(timespan);

                if (fullDay < 7)
                {
                    ImpressionData.Data.Add(impressionObject);

                    SaveImpressionData();
                    CheckImpressionStates();
                }

            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }

            // SayKit.trackEvent("d_impression_end", "[ type = " + impressionObject.Type + ", cpm = " + cpm + ", totalCount = " + ImpressionData.Data.Count + " ]");

            yield break;
        }

        private void SaveImpressionData()
        {
            try
            {
                lock (_lockObject)
                {
                    string cachePath = Application.persistentDataPath + "/" + _fileName;
                    var jsonData = JsonUtility.ToJson(ImpressionData);

                    System.IO.File.WriteAllText(cachePath, jsonData);
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log(exp.Message);
            }
        }


        public void ReadAllImpressions()
        {
            string cachePath = Application.persistentDataPath + "/" + _fileName;

            try
            {
                lock (_lockObject)
                {
                    var jsonData = System.IO.File.ReadAllText(cachePath);

                    SayKitDebug.Log("SPImpressionManager: " + jsonData);

                    ImpressionData = JsonUtility.FromJson<ImpressionData>(jsonData);
                    if (ImpressionData == null) { ImpressionData = new ImpressionData(); }
                }
            }
            catch (Exception exp)
            {
                SayKitDebug.Log("ReadAllImpressions error: " + exp.Message);
            }
        }

    }
}
