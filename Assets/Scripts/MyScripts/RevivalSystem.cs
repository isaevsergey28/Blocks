using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RevivalSystem : MonoBehaviour
{
    public static Action<bool> onButtonClick;

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(InvokeRevival);
    }

    private void InvokeRevival()
    {
        SayKit.showRewarded("ad_rewarded_revive", NotifyByClicking);
    }

    private void NotifyByClicking(bool isClicked)
    {
        onButtonClick?.Invoke(isClicked);
    }
}
