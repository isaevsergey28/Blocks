using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MVoxelizer;
using System.Linq;

public class FinalObject : MonoBehaviour
{
    public bool IsObjectBuilt { get; private set; } = false;

    private Voxel[] _allVoxels;
    private List<Voxel> _allSortedVoxels = new List<Voxel>();
    private Dictionary<Voxel, float> _dictVoxels = new Dictionary<Voxel, float>();
    private List<MeshRenderer> _allMeshRenderers = new List<MeshRenderer>();
    private int _currentBuiltVoxelIndex = 0;

    private void Awake()
    {
        _allVoxels = gameObject.GetComponentsInChildren<Voxel>();

        foreach(Voxel voxel in _allVoxels)
        {
            _dictVoxels.Add(voxel, voxel.gameObject.transform.position.y);
        }

        var sortedVoxels = from voxel in _dictVoxels
                 orderby voxel.Value
                 select voxel;

        foreach (var voxel in sortedVoxels)
        {
            _allSortedVoxels.Add(voxel.Key);
            _allMeshRenderers.Add(voxel.Key.GetComponent<MeshRenderer>());
        }
    }

    public Voxel[] GetAllVoxels()
    {
        return _allSortedVoxels.ToArray();
    }
    
    public MeshRenderer[] GetAllMeshRenderers()
    {
        return _allMeshRenderers.ToArray();
    }

    public void SetCurrentVoxelIndex(int voxelIndex)
    {
        _currentBuiltVoxelIndex = voxelIndex;
    }

    public int GetCurrentBuiltVoxelIndex()
    {
        return _currentBuiltVoxelIndex;
    }
}
