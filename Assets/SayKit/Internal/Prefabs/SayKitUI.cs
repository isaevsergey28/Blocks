
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SayKitInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SayKitUI : MonoBehaviour
{

    private static bool shouldShowPopupOnAwake = false;
    public static SayKitUI instance;

    public bool IsPopupShowed = false;


    public Sprite checkedOn;
    public Sprite checkedOff;
    public Text textTitle;
    public Text textTop;
    public Text textBottom;
    public Button buttonAccept;
    public Button buttonOk;

    public GameObject gdprPanel;

    bool wasAccepted = false;

    static Color colorAccepted = new Color(94f / 255, 135f / 255, 101f / 255, 1);
    static Color colorNotAccepted = new Color(243f / 255, 202f / 255, 82f / 255, 1);


    static string gdpr_popup_title = "gdpr_popup_title";
    static string gdpr_popup_text_top = "gdpr_popup_text_top";
    static string gdpr_popup_text_bottom = "gdpr_popup_text_bottom";
    static string gdpr_popup_button_accept = "gdpr_popup_button_accept";
    static string gdpr_popup_button_ok = "gdpr_popup_button_ok";
    static string gdpr_privacy_url = "gdpr_privacy_url";


    public static SayKitUI getInstance()
    {
        if (instance == null)
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].GetComponent<SayKitUI>() != null)
                {
                    rootObjects[i].name = "[SayKitUI]";
                    DontDestroyOnLoad(rootObjects[i]);
                    instance = rootObjects[i].GetComponent<SayKitUI>();

                    instance.buttonAccept.onClick.AddListener(instance.ClickAccept);
                    instance.buttonOk.onClick.AddListener(instance.ClickOk);
                    instance.textBottom.GetComponent<Button>().onClick.AddListener(instance.ClickTerms);
                    break;
                }
            }
        }

#if UNITY_EDITOR
        if (instance == null)
        {
            string message = "Please, add SayKitUI prefab on the top of first loading scene (build index = 0).";
            EditorUtility.DisplayDialog("SayKit Message", message, "OK");
            throw new System.Exception("SayKit: " + message);
        }
#endif

        return instance;

    }
    public static void ShowPopup()
    {
        getInstance().ShowPopupInternal();
    }

    public void ClickAccept()
    {
        wasAccepted = !wasAccepted;
        Refresh();
    }

    public void ClickOk()
    {
        IsPopupShowed = false;

        SayKit.grantGdprConsent();

        gdprPanel.SetActive(false);
        HideSayKitUI();
    }

    public void ClickTerms()
    {
        SayKit.trackEvent("gdpr_privacy_click");
        Application.OpenURL(SayKit.getLocalizedString(gdpr_privacy_url));
    }

    void Refresh()
    {
        buttonOk.interactable = wasAccepted;
        buttonAccept.GetComponentsInChildren<Image>()[1].sprite = wasAccepted ? checkedOn : checkedOff;
        buttonAccept.GetComponentsInChildren<Image>()[0].color = wasAccepted ? colorAccepted : colorNotAccepted;
    }

    void ShowPopupInternal()
    {
        IsPopupShowed = true;

        ShowSayKitUI();
        gdprPanel.SetActive(true);
        wasAccepted = false;

        textTitle.text = SayKit.getLocalizedString(gdpr_popup_title);
        textTop.text = SayKit.getLocalizedString(gdpr_popup_text_top);
        textBottom.text = SayKit.getLocalizedString(gdpr_popup_text_bottom);

        buttonOk.GetComponentInChildren<Text>().text = SayKit.getLocalizedString(gdpr_popup_button_ok);
        buttonAccept.GetComponentInChildren<Text>().text = SayKit.getLocalizedString(gdpr_popup_button_accept);

        Refresh();
    }


    public void ShowSayKitUI() { gameObject.SetActive(true); }

    public void HideSayKitUI()
    {
#if UNITY_IOS
        if (SayKitIDFAUI.instance == null)
        {
            if (!IsPopupShowed)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (!IsPopupShowed
              && (SayKitIDFAUI.instance?.IsPopupShowed == false))
            {
                gameObject.SetActive(false);
            }
        }
#else
        gameObject.SetActive(false);
#endif
    }

}