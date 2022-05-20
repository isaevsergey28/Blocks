using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildingRegistrar : MonoBehaviour
{
    public static BuildingRegistrar instance;
    public static event Action onVictory;

    private int _allFinalObjectsInCurrentLevel;
    private int _currentFinalObjectsBuiltNumber;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void RegisterFinalObject()
    {
        _allFinalObjectsInCurrentLevel++;
    }

    public void RegisterFinalConstructedObject()
    {
        _currentFinalObjectsBuiltNumber++;
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (_currentFinalObjectsBuiltNumber == _allFinalObjectsInCurrentLevel)
        {
            if(!(SayKit.showInterstitial("ad_interstitial_nextlevel", () => onVictory?.Invoke())))
            {
                onVictory?.Invoke();
            }
        }
    }
}
