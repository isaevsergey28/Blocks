using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCubeColor : MonoBehaviour
{
    private Material _material;

    private void OnEnable()
    {
        _material = GetComponent<MeshRenderer>().material;
        
    }
    public void SetColorMaterial(Color color)
    {
        _material.color = color;
    }
}
