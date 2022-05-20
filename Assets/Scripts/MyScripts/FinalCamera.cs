using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class FinalCamera : MonoBehaviour
{
    private CinemachineVirtualCamera _finalCameraScript;

    private void Start()
    {
        _finalCameraScript = GetComponent<CinemachineVirtualCamera>();
    }

    public void ActiveFinalCamera()
    {
        _finalCameraScript.Priority = 11;
    }
    public void InactiveFinalCamera()
    {
        _finalCameraScript.Priority = 9;
    }
}
