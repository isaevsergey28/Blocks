using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private GameObject[] _finalObjects;

    private BuildingProgress[] _buildingProgress;
    private Slider _slider;

    private void Start()
    {
        ConstructionCubeBehaviour.onReachingTarget += ChangeSliderValue;
        _buildingProgress = new BuildingProgress[_finalObjects.Length];

        for (int i = 0; i < _finalObjects.Length; i++)
        {
            _buildingProgress[i] = _finalObjects[i].GetComponent<BuildingProgress>();
        }

        _slider = GetComponent<Slider>();
        StartCoroutine(SetSliderMaxValue());
    }

    private void OnDisable()
    {
        ConstructionCubeBehaviour.onReachingTarget -= ChangeSliderValue;
    }

    private IEnumerator SetSliderMaxValue()
    {
        yield return null;
        for (int i = 0; i < _buildingProgress.Length; i++)
        {
            _slider.maxValue += _buildingProgress[i].GetVoxelsNumber();
        }
    }

    private void ChangeSliderValue(GameObject cube = null)
    {
        _slider.value++;
    }
}
