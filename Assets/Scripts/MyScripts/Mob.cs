using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Mob : MonoBehaviour
{
    [SerializeField] protected int _maxHealth = 100;

    public static Action<GameObject, int> onChangedHealth;

    protected AnimationStateSystem _animStateSystem;
    [SerializeField] protected int _currentHealth;
    protected SlashHitSystem _slashHit;
    
    public bool isHealingMob = false;

    public virtual void GiveDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        onChangedHealth?.Invoke(this.gameObject, damage);
    }

    public AnimationStateSystem GetAnimStateSystem()
    {
        return _animStateSystem;
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public void StopHitAnim()
    {
        _animStateSystem.StopAnimByName("Hit");
    }

    public void PlayShashParticles()
    {
        _slashHit.PlayParticleSystem();
    }
}
