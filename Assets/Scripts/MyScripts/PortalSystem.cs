using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSystem : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;

    private void Start()
    {
        BuildingRegistrar.onVictory += StopSpawning;
        PlayerMovement.onStartMove += SpawnFirstEnemy;
    }

    private void OnDisable()
    {
        BuildingRegistrar.onVictory -= StopSpawning;
        PlayerMovement.onStartMove -= SpawnFirstEnemy;
    }

    
    public void SpawnNextEnemy()
    {
        StartCoroutine(CoolDownBetweenSpawn());
    }

    private IEnumerator CoolDownBetweenSpawn()
    {
        yield return new WaitForSeconds(30f);
        Instantiate(_enemyPrefab, transform.position, Quaternion.identity, transform);
    }

    private void SpawnFirstEnemy()
    {
        Instantiate(_enemyPrefab, transform.position, Quaternion.identity, transform);
        PlayerMovement.onStartMove -= SpawnFirstEnemy;
    }

    private void StopSpawning()
    {
        Destroy(this);
    }
}
