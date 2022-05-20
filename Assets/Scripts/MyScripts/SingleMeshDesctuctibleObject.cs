using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleMeshDesctuctibleObject : MonoBehaviour
{
    [SerializeField] private GameObject _mainDesctuctibleObject;

    public void ChangeObjects()
    {
        _mainDesctuctibleObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
