using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubesLayersControl : MonoBehaviour
{
    [SerializeField] private int _resourcesAmountForCubesLayer = 17;
    [SerializeField] private int _maxCubesInHeight = 10;
    [SerializeField] private int _maxCubesNumberRows = 3;
    [SerializeField] private GameObject _cubesPrefab;
    [SerializeField] private List<GameObject> _allCubesLayers = new List<GameObject>();

    private int _resourceAmountForDeleteOneCubeLayer;
    private int _lastNumberCubesLayer = 0;

    private void Awake()
    {
        int _yPosMultiplier = 0;
        int _zPosMultiplier = 0;

        for (int i = 0; i < _maxCubesNumberRows; i++)
        {
            for (int j = 0; j < _maxCubesInHeight; j++)
            {
                GameObject newCubesLayer = Instantiate(_cubesPrefab, transform.position, Quaternion.identity, transform);
                newCubesLayer.transform.localPosition = new
                    Vector3(0, (transform.position.y + 1f) + 0.15f * _yPosMultiplier, -0.3f - 0.125f * _zPosMultiplier);
                newCubesLayer.transform.localRotation = Quaternion.Euler(0, 0, 0);
                _allCubesLayers.Add(newCubesLayer);
                _yPosMultiplier++;
                newCubesLayer.SetActive(false);
            }
            _yPosMultiplier = 0;
            _zPosMultiplier++;
        }
    }

    public void CheckForSpawn(int resourceAmount)
    {
        if(resourceAmount % _resourcesAmountForCubesLayer == 0 && _lastNumberCubesLayer < _maxCubesInHeight * _maxCubesNumberRows)
        {
            SpawnCubesLayerBehindBack();
        }
    }

    public void CheckForDelete(int resourceAmount)
    {
        if (_resourceAmountForDeleteOneCubeLayer == resourceAmount && resourceAmount != 0)
        {
            DeleteCubesLayerBehindBack();
        }
    }

    private void SpawnCubesLayerBehindBack()
    {
        _resourceAmountForDeleteOneCubeLayer += _resourcesAmountForCubesLayer;
        _resourceAmountForDeleteOneCubeLayer = Mathf.Clamp(_resourceAmountForDeleteOneCubeLayer, 0, (_maxCubesNumberRows * _maxCubesInHeight * _resourcesAmountForCubesLayer) - 1);
        _allCubesLayers[_lastNumberCubesLayer++].SetActive(true);
    }

    private void DeleteCubesLayerBehindBack()
    {
        _resourceAmountForDeleteOneCubeLayer -= _resourcesAmountForCubesLayer;
        _allCubesLayers[--_lastNumberCubesLayer].SetActive(false);
    }
}
