using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Mob _mob;
    private Slider _healthBar;
    private PortalSystem _portal;

    private void Start()
    {
        Mob.onChangedHealth += ChangeValue;
        Player.onHealthAdded += AddHealth;
        _portal = FindObjectOfType<PortalSystem>();
        if (_portal == null)
        {
            gameObject.SetActive(false);
        }
        _mob = transform.parent.parent.GetComponent<Mob>();
        _healthBar = GetComponent<Slider>();
        _healthBar.value = _healthBar.maxValue = _mob.GetMaxHealth();
    }

    private void OnDisable()
    {
        Mob.onChangedHealth -= ChangeValue;
        Player.onHealthAdded -= AddHealth;
    }

    private void ChangeValue(GameObject mob, int damage)
    {
        if(_mob.gameObject == mob)
        {
            _healthBar.value -= damage;
        }
    }
    
    private void AddHealth(GameObject mob, int damage)
    {
        if(_mob.gameObject == mob)
        {
            _healthBar.value += damage;
        }
    }
}
