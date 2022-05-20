using UnityEngine;
using System.Collections;

namespace SayKitInternal {

    public class FpsCounter {

        static int lastFrames = 0;
        static float lastTime = 0;
        static int memoryCounter = 0;

        static public void update() {
            lastFrames++;
        }

        public static void reportFps() {
			float timeSinceUpdate = Time.time - lastTime;

            if (timeSinceUpdate > 1.0f) {
                float fps = lastFrames / timeSinceUpdate;
                
                lastTime = Time.time;
                lastFrames = 0;

                if (fps > 0) {
                    AnalyticsEvent.trackEvent("fps", (int)fps);
                }
            }
        }

        static public IEnumerator reportFpsRoutine() {
            lastTime = Time.time;
            lastFrames = 0;

            while (true) {
                memoryCounter++;

                yield return new WaitForSecondsRealtime(20f);
                reportFps();

                if (memoryCounter >= 3)
                {
                    memoryCounter = 0;

                    AnalyticsEvent.trackAvailableMemory();
                }
            }
        }
    }

    public class DebugFpsCounter {

        private static float _deltaTime = 0.0f;
        private static readonly float _minFPSRate = 20f;
        private static readonly int _minSpikes = 5;

        private static int _screenSpikes = 0;
        private static int _screenTimestamp = 0;
        private static string _screenName = "";


        public static void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }


        public static void UpdateScreen(string screenName)
        {
            if (_screenName.Length > 0 && _screenSpikes > _minSpikes)
            {
                var timestamp = Utils.currentTimestamp - _screenTimestamp;
                AnalyticsEvent.trackEvent("fps_screen", _screenSpikes, timestamp, _screenName);
            }

            _screenTimestamp = Utils.currentTimestamp;
            _screenName = screenName;
            _screenSpikes = 0;
        }


        public static IEnumerator StartDebugFPSRoutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(1);

                float fps = Mathf.Round(1.0f / _deltaTime);

                if (fps <= _minFPSRate)
                {
                    if (_screenSpikes < int.MaxValue)
                    {
                        _screenSpikes++;
                    }
                }
            }
        }

    }

}

