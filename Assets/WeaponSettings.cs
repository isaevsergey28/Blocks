using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/CreateNewWeapon", order = 1)]
public class WeaponSettings : ScriptableObject
{
    [SerializeField] private DiceSetting[] cubeSettings;

    public DiceSetting GetWeaponSettings(WeaponEnum.Weapon weapon)
    {
        return cubeSettings.FirstOrDefault(c => c.weapon == weapon);
    }

    [Serializable]
    public class DiceSetting
    {
        [field: SerializeField] public WeaponEnum.Weapon weapon { get; private set; }

        [field: SerializeField] public GameObject WeaponObject { get; private set; }
    }
}
