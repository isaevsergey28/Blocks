using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMob : MonoBehaviour
{
    private Transform _target;

    private void Start()
    {
        _target = FindObjectOfType<MainCamera>().gameObject.transform;
    }

    private void Update()
    {
        transform.LookAt(_target);
    }
}
