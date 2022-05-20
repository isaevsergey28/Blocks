using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVoxelizer;
using DG.Tweening;
using System;

public class FinalObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject _backpackObject;

    public static event Action onReleaseCube;

    private ConstructionCubesSpawner _cubesSpawner;
    private Voxel[] _allVoxels;
    private MeshRenderer[] _voxelsMeshRenderers;
    private Backpack _backpack;
    private FinalObject _finalObjectScript;
    private bool _isCanBuild = false;
    private Coroutine _buildObjectRoutine;
    private int _currentVoxelIndex = 0;

    private void Start()
    {
        BuildZone.onTriggerEnterBuildZoneEvent += StartBuilding;
        BuildZone.onTriggerExitBuildZoneEvent += StopBuilding;
        _backpack = _backpackObject.GetComponent<Backpack>();
        _cubesSpawner = GetComponent<ConstructionCubesSpawner>();
    }
    
    private void OnDisable()
    {
        BuildZone.onTriggerEnterBuildZoneEvent -= StartBuilding;
        BuildZone.onTriggerExitBuildZoneEvent -= StopBuilding;
    }

    private void StartBuilding(FinalObject finalObject)
    {
        _isCanBuild = true;
        _finalObjectScript = finalObject;
        _allVoxels = _finalObjectScript.GetAllVoxels();
        _voxelsMeshRenderers = _finalObjectScript.GetAllMeshRenderers();
        _currentVoxelIndex = _finalObjectScript.GetCurrentBuiltVoxelIndex();
        _buildObjectRoutine = StartCoroutine(BuildObject());
    }

    private void StopBuilding()
    {
        _isCanBuild = false;
        _finalObjectScript?.SetCurrentVoxelIndex(_currentVoxelIndex);
    }
    
    private IEnumerator BuildObject()
    {
        yield return new WaitForSeconds(0.01f);
        for (int i = _currentVoxelIndex; _currentVoxelIndex < _allVoxels.Length;)
        {
            for(int j = 0; j < 20 && _currentVoxelIndex < _allVoxels.Length; j++)
            {
                if (_backpack.GetResourceAmount() < 1 || !_isCanBuild)
                {
                    i = _currentVoxelIndex;
                    StopCoroutine(_buildObjectRoutine);
                }
                if (!(_voxelsMeshRenderers[_currentVoxelIndex].enabled))
                {
                    _cubesSpawner.RunCube(_allVoxels[_currentVoxelIndex].gameObject.transform.position, _voxelsMeshRenderers[_currentVoxelIndex]);
                    onReleaseCube?.Invoke();
                }
                _currentVoxelIndex++;
            }
            i = _currentVoxelIndex;
            yield return null;
        }
    }
    

}
