using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class BuildingProgress : MonoBehaviour
{
    public static event Action<GameObject> onBuilded;

    [SerializeField] private GameObject _progressObject;

    private FinalObject _finalObject;
    private FinalObject _activeFinalObject;
    private TextMeshProUGUI _builidngProgressText;
    private int _builidngProgress = 0;
    private int _voxelsReachedTarget = 0;
    private int _voxelsNumber;

    private void Start()
    {
        BuildZone.onTriggerEnterBuildZoneEvent += SetActiveProgress;
        ConstructionCubeBehaviour.onReachingTarget += IncreaseVoxelReachedTarget;
        _builidngProgressText = _progressObject.GetComponentInChildren<TextMeshProUGUI>();
        _finalObject = GetComponent<FinalObject>();
        _voxelsNumber = _finalObject.GetAllVoxels().Length;
        BuildingRegistrar.instance.RegisterFinalObject();
    }

    private void OnDisable()
    {
        BuildZone.onTriggerEnterBuildZoneEvent -= SetActiveProgress;
        ConstructionCubeBehaviour.onReachingTarget -= IncreaseVoxelReachedTarget;
    }

    public int GetBuildingProgress()
    {
        return _builidngProgress;
    }

    public FinalObject GetFinalObject()
    {
        return _finalObject;
    }

    public int GetVoxelsNumber()
    {
        return _voxelsNumber;
    }

    private void SetActiveProgress(FinalObject activeFinalObject)
    {
        _activeFinalObject = activeFinalObject;
    }
    private void IncreaseVoxelReachedTarget(GameObject cube = null)
    {
        ChangeProgressText();
    }

    private void ChangeProgressText()
    {
        if (_finalObject == _activeFinalObject)
        {
            _voxelsReachedTarget++;
            _builidngProgress = (int)(_voxelsReachedTarget * 100) / _voxelsNumber;
            _builidngProgress = Mathf.Clamp(_builidngProgress, 0, 100);
            _builidngProgressText.text = _builidngProgress + " %";
            if (_builidngProgress == 100)
            {
                GameObject root = transform.parent.parent.gameObject;
                StopChanging(root);
                onBuilded?.Invoke(root);
                BuildingRegistrar.instance.RegisterFinalConstructedObject();
            }
        }
    }

    private static void StopChanging(GameObject root)
    {
        root.GetComponent<BuildZone>().StopBuilding();
        root.GetComponent<BoxCollider>().enabled = false;
    }
}
