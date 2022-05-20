using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;


namespace SayKitInternal {

    class UI {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void init() {
             
            SayKitUI.getInstance();
            SayKitBanner.getInstance();
        }
    }
}