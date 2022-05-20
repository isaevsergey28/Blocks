using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackTower : MonoBehaviour
{
    [SerializeField] private int treatmentFrequencySec = 0;
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private GameObject towerHead = null;
    public bool isStayingInCollider = false;
    public GameObject attackingTarget = null;
    private float homeY;
    bool isShoot;
    public bool Catcher = false;

    private void OnEnable()
    {
        Enemy.onEnemyDeath += OnEnemyDeath;
    }

    private void OnDisable()
    {
        Enemy.onEnemyDeath -= OnEnemyDeath;
    }

    private void Start()
    {
        homeY = towerHead.transform.localRotation.eulerAngles.y;
    }

    private void OnEnemyDeath()
    {
        //StopAllCoroutines();
        isStayingInCollider = false;
        attackingTarget = null;
    }

    private void Update()
    {
        if (attackingTarget)
        {
            Vector3 dir = attackingTarget.transform.position - towerHead.transform.position;
            dir.y = 0; 
            Quaternion rot = Quaternion.LookRotation(dir);                
            towerHead.transform.rotation = Quaternion.Slerp( towerHead.transform.rotation, rot, 5 * Time.deltaTime);
        }
        else
        {
            
            Quaternion home = new Quaternion (0, homeY, 0, 1);
            
            towerHead.transform.rotation = Quaternion.Slerp(towerHead.transform.rotation, home, Time.deltaTime);                        
        }
        
        if (!isShoot)
        {
            StartCoroutine(shoot());

        }

        
        if (Catcher == true)
        {
            if (!attackingTarget)
            {
                StopCatcherAttack();
            }

        }
    }
    
    IEnumerator shoot()
    {
        isShoot = true;
        yield return new WaitForSeconds(treatmentFrequencySec);


        if (attackingTarget && Catcher == false)
        {
            GameObject b = GameObject.Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
            b.GetComponent<TowerBullet>().target = attackingTarget.transform;
            b.GetComponent<TowerBullet>().twr = this;
          
        }

        isShoot = false;
    }
    
    void StopCatcherAttack()
    {                
        attackingTarget = null;
    } 

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            isStayingInCollider = true;
            //StartCoroutine(AttackTowerCor(enemy));
            attackingTarget = enemy.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Enemy enemy))
        {
            //StopAllCoroutines();
            isStayingInCollider = false;
            attackingTarget = null;
        }
    }
}
