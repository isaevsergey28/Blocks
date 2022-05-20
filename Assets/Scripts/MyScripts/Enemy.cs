using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : Mob
{
    public static Action onEnemyDeath;
    public static Action<int> onHealingEnemyDeath;

    private PortalSystem _portal;
    [SerializeField] private UIMob _uiMob;
    private EnemyMovement _movement;
    
    [SerializeField] private int healValue = 10;

    private void Start()
    {
        Player.onPlayerDeath += StopEnemy;
        Player.onPlayerRevive += DestroyByRebirth;
        BuildingRegistrar.onVictory += StopEnemy;

        _animStateSystem = GetComponent<AnimationStateSystem>();
        
        if (!isHealingMob)
        {
            _portal = transform.parent.GetComponent<PortalSystem>();
        }
     
        _uiMob = transform.GetComponentInChildren<UIMob>();
        _movement = GetComponent<EnemyMovement>();
        _slashHit = GetComponentInChildren<SlashHitSystem>();
        _currentHealth = _maxHealth;
    }
    
    private void OnDisable()
    {
        Player.onPlayerDeath -= StopEnemy;
        Player.onPlayerRevive -= DestroyByRebirth;
        BuildingRegistrar.onVictory -= StopEnemy;
    }

    public override void GiveDamage(int damage)
    {
        base.GiveDamage(damage);
        if (_currentHealth == 0)
        {
            onEnemyDeath?.Invoke();
            _animStateSystem.ChangeState(State.Death);
            GetComponent<CapsuleCollider>().enabled = false;
            _uiMob.gameObject.SetActive(false);
          
            if (!isHealingMob)
            {
                StartCoroutine(DestroyWait(5f));
                Destroy(_movement);
            }
            else
            {
                onHealingEnemyDeath?.Invoke(healValue);
                Destroy(gameObject);
            }
        }
    }
    

    public IEnumerator DestroyWait(float time)
    {
        yield return new WaitForSeconds(time);
        _portal.SpawnNextEnemy();
        Destroy(gameObject);
    }

    private void StopEnemy()
    {
        _movement.Stop();
        GiveDamage(100);
        StopAllCoroutines();
        GetComponentInChildren<BoxCollider>().enabled = false;
    }

    private void DestroyByRebirth()
    {
        onEnemyDeath?.Invoke();
        StartCoroutine(DestroyWait(0f));
    }
}
