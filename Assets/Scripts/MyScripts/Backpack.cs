using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    public static event Action onUsingResources;

    private int _resourceAmount;
    private CubesLayersControl _cubesLayersControl;

    private void Start()
    {
        _resourceAmount = 0;
        DestructibleObjectVoxel.onSuckingCube += IncreaseResourceAmount;
        FinalObjectCreator.onReleaseCube += DecreaseResourceAmount;
        _cubesLayersControl = GetComponent<CubesLayersControl>();
    }

    private void OnDisable()
    {
        DestructibleObjectVoxel.onSuckingCube -= IncreaseResourceAmount;
        FinalObjectCreator.onReleaseCube -= DecreaseResourceAmount;
    }

    public int GetResourceAmount()
    {
        return _resourceAmount;
    }

    public void DecreaseResourceAmount()
    {
        if(_resourceAmount > 0)
        {
            _resourceAmount--;
            _cubesLayersControl.CheckForDelete(_resourceAmount);
            onUsingResources?.Invoke();
        }
    }

    private void IncreaseResourceAmount()
    {
        _resourceAmount++;
        _cubesLayersControl.CheckForSpawn(_resourceAmount);
    }
}
