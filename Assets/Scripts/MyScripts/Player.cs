using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Mob
{
    public static Action onPlayerDeath;
    public static Action onPlayerRevive;
    public static Action<GameObject, int> onHealthAdded;

    private bool _isAlive = true;

    private void Start()
    {
        Enemy.onEnemyDeath += StopHurtAnim;
        Enemy.onHealingEnemyDeath += OnHealingEnemyDeath;
        RevivalSystem.onButtonClick += RevivePlayer;
        _animStateSystem = GetComponent<AnimationStateSystem>();
        _slashHit = GetComponentInChildren<SlashHitSystem>();
        _currentHealth = _maxHealth;
    }

    public void OnHealingEnemyDeath(int value)
    {
        _currentHealth += value;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        onHealthAdded?.Invoke(gameObject,value);
    }

    private void OnDisable()
    {
        Enemy.onEnemyDeath -= StopHurtAnim;
        RevivalSystem.onButtonClick -= RevivePlayer;
        Enemy.onHealingEnemyDeath -= OnHealingEnemyDeath;
    }
    

    public override void GiveDamage(int damage)
    {
        base.GiveDamage(damage);
        if (_currentHealth == 0 && _isAlive)
        {
            _isAlive = false;
            _animStateSystem.ChangeState(State.Death);
            GetComponent<PlayerMovement>().StopPlayer();
            onPlayerDeath?.Invoke();
        }
    }

    private void RevivePlayer(bool isRevive)
    {
        if(isRevive)
        {
            _isAlive = true;
            _animStateSystem.ChangeState(State.Revive);
            IncreasePlayerHealth();
            GetComponent<PlayerMovement>().AllowMovement();
            onPlayerRevive?.Invoke();
        }
    }

    private void IncreasePlayerHealth()
    {
        _currentHealth = _maxHealth;
        onChangedHealth?.Invoke(this.gameObject, -_currentHealth);
    }

    private void StopHurtAnim()
    {
        _animStateSystem.StopAnimByName("Hurt");
    }
}
