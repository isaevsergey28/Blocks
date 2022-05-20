using System;
using System.Collections.Generic;
using UnityEngine;

namespace SayKitInternal {
    class AnalyticsTenjin {

        static public void init() {
            if (SayKit.config.tenjinApiKey != "") {
                BaseTenjin instance = Tenjin.getInstance(SayKit.config.tenjinApiKey);
                instance.OptIn();
                instance.Connect();

                // instance.GetDeeplink(DeferredDeeplinkCallback);
            }
        }

        static public void sendCustomEvent(string eventName) {
            if (SayKit.config.tenjinApiKey != "") {
                BaseTenjin instance = Tenjin.getInstance(SayKit.config.tenjinApiKey);
                instance.SendEvent(eventName);
            }
        }

        static public void completedAndroidPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string Receipt, string Signature) {
            if (SayKit.config.tenjinApiKey != "") {
                BaseTenjin instance = Tenjin.getInstance(SayKit.config.tenjinApiKey);
                instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, null, Receipt, Signature);
            }
        }

        static public void completedIosPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string TransactionId, string Receipt) {
            if (SayKit.config.tenjinApiKey != "") {
                BaseTenjin instance = Tenjin.getInstance(SayKit.config.tenjinApiKey);
                instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, TransactionId, Receipt, null);
            }
        }



        public static void DeferredDeeplinkCallback(Dictionary<string, string> data)
        {
            bool clicked_tenjin_link = false;
            bool is_first_session = false;

            if (data.ContainsKey("clicked_tenjin_link"))
            {
                //clicked_tenjin_link is a BOOL to handle if a user clicked on a tenjin link
                clicked_tenjin_link = Convert.ToBoolean(data["clicked_tenjin_link"]);
            }

            if (data.ContainsKey("is_first_session"))
            {
                //is_first_session is a BOOL to handle if this session for this user is the first session
                is_first_session = Convert.ToBoolean(data["is_first_session"]);
            }


            if (clicked_tenjin_link && is_first_session)
            {
                string result = "{ ";
                foreach (var item in data)
                {
                    result += string.Format("\"{0}\":\"{1}\", ", item.Key, item.Value);
                }
                result += " }";

                SayKitDebug.Log("Tenjin DeferredDeeplinkCallback: " + result);
                AnalyticsEvent.trackEvent("deeplink_data", result);
            }
            
        }


    }
}