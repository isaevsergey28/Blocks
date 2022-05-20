using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //[SerializeField] private WeaponSettings _weaponSettings = null;
    //
    // [SerializeField] private WeaponEnum.Weapon weaponType;
    // private WeaponEnum.Weapon prevWeaponType;
    //
    // [SerializeField] private bool isDebug = false;
    // private void Start()
    // {
    //     _weaponSettings.GetWeaponSettings(weaponType).WeaponObject.gameObject.SetActive(true);
    //     prevWeaponType = weaponType;
    // }
    //
    // private void Update()
    // {
    //     if (isDebug)
    //     {
    //         if (prevWeaponType != weaponType)
    //         {
    //             _weaponSettings.GetWeaponSettings(prevWeaponType).WeaponObject.gameObject.SetActive(false);
    //             _weaponSettings.GetWeaponSettings(weaponType).WeaponObject.gameObject.SetActive(true);
    //             prevWeaponType = weaponType;
    //         }
    //     }
    // }

    [SerializeField] private List<GameObject> weapons = new List<GameObject>();
    [SerializeField] [Range(0,24)] private int index = 0;
    private int _activeWeaponIndex = 0;

    private void Start()
    {
        _activeWeaponIndex = index;
        weapons[index].SetActive(true);
    }

    private void Update()
    {
        if(_activeWeaponIndex != index)
        {
            foreach (var weapon in weapons)
            {
                weapon.SetActive(false);
            }
            weapons[index].SetActive(true);
            _activeWeaponIndex = index;
        }
    }
}
