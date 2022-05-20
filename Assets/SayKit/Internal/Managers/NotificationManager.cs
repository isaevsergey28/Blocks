using UnityEngine;
using System.Collections;

#if UNITY_IOS && SAYKIT_NOTIFICATIONS && !UNITY_CLOUD_BUILD
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using LocalNotification = UnityEngine.iOS.LocalNotification;

namespace SayKitInternal {

    class NotificationManager {

        static bool tokenSent = false;
        
        static public IEnumerator initRoutine() {

            // some logic for notifications popup delay

            NotificationServices.RegisterForNotifications(
            NotificationType.Alert |
            NotificationType.Badge |
            NotificationType.Sound, true);

            while (true) {
		
			    byte[] token = NotificationServices.deviceToken;

                if (token != null)
                {
                    if (!tokenSent) {
                        
                        NotificationServices.ClearRemoteNotifications();
                        
                        tokenSent = true;
                        
                        //Debug.Log("token is " + System.BitConverter.ToString(token));
                        string hexToken = System.BitConverter.ToString(token).Replace("-", "");

                        SayKit.trackEvent("notification_token", hexToken);

                        yield break;
                    }         
                }
                yield return new WaitForSecondsRealtime(1);
            }
		}
    }
}

#elif UNITY_ANDROID && SAYKIT_NOTIFICATIONS

namespace SayKitInternal {

    class NotificationManager {

         private static readonly AndroidJavaClass SayKitNotificationsJava = new AndroidJavaClass("by.saygames.SayKitNotifications");
                
        static public IEnumerator initRoutine() {

            // some logic for notifications popup delay

            SayKitNotificationsJava.CallStatic("init");
           
            while (true) {
			    string token = SayKitNotificationsJava.CallStatic<string>("getToken");
                if (token != "")
                {
                    SayKit.trackEvent("notification_token", token);
                    yield break;
                }
                yield return new WaitForSecondsRealtime(1);
            }
		}
    }
}

#endif