using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashHitSystem : MonoBehaviour
{
    [SerializeField] private GameObject _slashHit;

    public void PlayParticleSystem()
    {
        GameObject slashHit = Instantiate(_slashHit, transform.position, Quaternion.identity, transform);
        slashHit.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DestroyShashParticles(slashHit));
    }

    private IEnumerator DestroyShashParticles(GameObject slashHit)
    {
        yield return new WaitForSeconds(1f);
        Destroy(slashHit);
    }
}
