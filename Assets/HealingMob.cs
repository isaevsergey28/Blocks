using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingMob : Mob
{
    public static Action onEnemyDeath;
    
    private UIMob _uiMob;

    private void Start()
    {
        _uiMob = transform.GetComponentInChildren<UIMob>();
       
        _currentHealth = _maxHealth;
    }
    
    public override void GiveDamage(int damage)
    {
        base.GiveDamage(damage);
        if (_currentHealth == 0)
        {
            onEnemyDeath?.Invoke();
            _uiMob.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
