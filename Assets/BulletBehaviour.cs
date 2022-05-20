using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [SerializeField] private int bulletDamage = 0;
    public Transform target = null;


    private void Start()
    {
        target = GetComponentInParent<AttackTower>().attackingTarget.transform;
        
        if (target != null)
        {
            transform.DOMove(new Vector3(target.transform.position.x, 
                target.transform.position.y + 1f, target.transform.position.z), 1f).SetEase(Ease.Linear);
            Destroy(gameObject, 5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
       
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.GiveDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}
