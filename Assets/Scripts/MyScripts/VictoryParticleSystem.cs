using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryParticleSystem : MonoBehaviour
{
    [SerializeField] private GameObject _victoryPSPrefab;

    private void Start()
    {
       BuildingProgress.onBuilded += PlayParticleSystem;
    }

    private void OnDisable()
    {
        BuildingProgress.onBuilded -= PlayParticleSystem;
    }

    private void PlayParticleSystem(GameObject parent)
    {
        if(parent == this.transform.parent.gameObject)
        {
            GameObject victoryPS = Instantiate(_victoryPSPrefab, transform.position, Quaternion.identity, transform);
            victoryPS.GetComponent<ParticleSystem>().Play();
        }
        
        CoinsCounter.Instance.AddCoins(50);
    }
}
