﻿using UnityEngine;
using System.Collections;

public class EnemyBullet : MonoBehaviour
{
    public float Speed;
    public Transform target;
    public GameObject impactParticle; // bullet impact    
    public Vector3 impactNormal; 
    Vector3 lastBulletPosition; 
    public Enemy twr;
    float i = 0.05f; // delay time of bullet destruction
    [SerializeField] private int bulletDamage = 0;

    void Update()
    {

        // Bullet move

        if (target)
        {

            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * Speed); 
            lastBulletPosition = target.transform.position; 

        }

        // Move bullet ( enemy was disapeared )

        else
        {

            transform.position = Vector3.MoveTowards(transform.position, lastBulletPosition, Time.deltaTime * Speed); 

            if (transform.position == lastBulletPosition)
            {
                Destroy(gameObject, i);

                // Bullet hit ( enemy was disapeared )

                if (impactParticle != null) // poison tower showed error
                {
                    impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
                    Destroy(impactParticle, 3);
                    return;
                }
            }

        }
    }
    
    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.transform == target)
        {
            target.gameObject.GetComponent<Enemy>().GiveDamage(bulletDamage);
            Destroy(gameObject, i); // destroy bullet
            impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
            impactParticle.transform.parent = target.transform;
            Destroy(impactParticle, 3);
            return;
        }
    }

}




