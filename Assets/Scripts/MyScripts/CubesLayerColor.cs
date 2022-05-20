using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubesLayerColor : MonoBehaviour
{
    [SerializeField] private GameObject[] _childrenCubes;

    private void Start()
    {
        foreach(GameObject cube in _childrenCubes)
        {
            cube.GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        }
    }
}