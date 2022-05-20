
using UnityEngine;
using UnityEngine.UI;


namespace SayKitInternal
{
    public class SayKitIDFAUI : MonoBehaviour
    {

        public static SayKitIDFAUI instance;
        public bool IsPopupShowed = false;

        public Text textDescription;
        public Button buttonOk;

        public Text tutorialTextDescription;
        public Button tutorial_buttonInfo;

        public GameObject infoPanel;
        public Text info_description;
        public Button info_buttonOk;

        public GameObject skad_7_boy;
        public GameObject skad_7_boy_ipad;

        private CanvasGroup _canvas;



        private static readonly string _skad_popup_text = "skad_unity_popup_text";
        private static readonly string _skad_popup_ok = "skad_unity_popup_ok";

        private static readonly string _skad_tut_popup_text = "skad_unity_tut_popup_text";
        private static readonly string _skad_tut_popup_btn_info = "skad_unity_tut_btn_info";

        private static readonly string _skad_popup_info_description = "skad_popup_info_text";
        private static readonly string _skad_popup_info_ok = "skad_unity_popup_info_ok";


        public static SayKitIDFAUI getInstance(string unityPopupLoyout)
        {
            if (instance == null)
            {
                var components = SayKitUI.getInstance().GetComponentsInChildren(typeof(SayKitIDFAUI), true);
                SayKitDebug.Log("SayKitIDFAUI: " + components.Length);

                CheckDevice();

                for (int i = 0; i < components.Length; i++)
                {

                    if (components[i].name == unityPopupLoyout)
                    {
                        DontDestroyOnLoad(components[i]);
                        instance = components[i].GetComponent<SayKitIDFAUI>();

                        instance.buttonOk.onClick.AddListener(instance.ClickOk);


                        if (SayKit.remoteConfig.ads_settings.skad_popup_layout_unity == 72)
                        {
                            if (instance.tutorial_buttonInfo != null)
                            {
                                instance.tutorial_buttonInfo.onClick.AddListener(instance.TutorialButtonInfoClick);
                            }

                            if (instance.info_buttonOk != null)
                            {
                                instance.info_buttonOk.onClick.AddListener(instance.InfoButtonOkClick);
                            }
                        }

                        SayKitDebug.Log("SayKitIDFAUI: selected" + components[i].name);
                    }
                }
            }

            return instance;
        }


        public void ClickOk()
        {
            IsPopupShowed = false;

            SayKitUI.instance.HideSayKitUI();
            gameObject.SetActive(false);

            SayKitDebug.Log("SayKitIDFAUI: ClickOk");
#if UNITY_IOS
            RuntimeInfoManager.OnUnityPopupResult(true);
#endif

        }

        public void ClickCancel()
        {
            SayKitDebug.Log("SayKitIDFAUI: ClickCancel ");

            IsPopupShowed = false;

            SayKitUI.instance.HideSayKitUI();
            gameObject.SetActive(false);

#if UNITY_IOS
            RuntimeInfoManager.OnUnityPopupResult(false);
#endif

        }


        public void TutorialButtonInfoClick()
        {
            infoPanel.SetActive(true);
        }


        public void InfoButtonOkClick()
        {
            infoPanel.SetActive(false);
        }



        private bool _startShowing = false;
        public void ShowPopup()
        {
            IsPopupShowed = true;
            SayKitDebug.Log("SayKitIDFAUI: ShowPopup");

            SayKitUI.instance.ShowSayKitUI();


            _canvas = gameObject.GetComponent<CanvasGroup>();
            if (_canvas != null) { _canvas.alpha = 0; }

            _startShowing = true;
            gameObject.SetActive(true);


            if (textDescription != null)
            {
                textDescription.text = SayKit.getLocalizedString(_skad_popup_text);
            }


            buttonOk.GetComponentInChildren<Text>().text = SayKit.getLocalizedString(_skad_popup_ok);

            if (SayKit.remoteConfig.ads_settings.skad_popup_layout_unity == 72)
            {
                tutorial_buttonInfo.GetComponentInChildren<Text>().text = SayKit.getLocalizedString(_skad_tut_popup_btn_info);

                info_description.text = SayKit.getLocalizedString(_skad_popup_info_description);
                info_buttonOk.GetComponentInChildren<Text>().text = SayKit.getLocalizedString(_skad_popup_info_ok);

                if (_isIPadDevice)
                {
                    skad_7_boy.SetActive(false);
                    skad_7_boy_ipad.SetActive(true);
                }
                else
                {
                    skad_7_boy.SetActive(true);
                    skad_7_boy_ipad.SetActive(false);
                }
            }


            if (tutorialTextDescription != null)
            {
                tutorialTextDescription.text = SayKit.getLocalizedString(_skad_tut_popup_text);
            }

        }


        void Update()
        {
            if (_startShowing)
            {
                if (_canvas?.alpha < 1)
                {
                    var alfa = _canvas.alpha + 0.1f;
                    if (alfa >= 1)
                    {
                        _canvas.alpha = 1;
                        _startShowing = false;
                    }
                    else
                    {
                        _canvas.alpha = alfa;
                    }
                }
                else
                {
                    _startShowing = false;
                }
            }
        }


        private static bool _isIPadDevice = false;
        private static void CheckDevice()
        {
#if UNITY_IOS
            _isIPadDevice = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
#endif
        }

    }
}
