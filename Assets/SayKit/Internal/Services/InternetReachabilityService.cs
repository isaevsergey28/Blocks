using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SayKitInternal
{
	public class InternetReachabilityService
	{

        private static bool _isInternetStatusChecked = false;
        public static bool IsInternetStatusChecked
        {
            get
            {
                return _isInternetStatusChecked;
            }
        }

        private static NetworkReachability _internetReachability = NetworkReachability.NotReachable;
        public static NetworkReachability InternetReachability
		{
			get
            {
                return _internetReachability;
			}
		}


        public static IEnumerator CheckInternetConnection()
        {
            string url = "https://app.saygames.io/runtime";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 3;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.Log("SayKit: DownloadAvailableDatabaseList NetworkError: " + webRequest.error);
                    _internetReachability = NetworkReachability.NotReachable;
                }
                else
                {
                    _internetReachability = NetworkReachability.ReachableViaLocalAreaNetwork;
                }

                _isInternetStatusChecked = true;
            }
        }

    }
}