using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] protected float _explosionForce = 100f;
    [SerializeField] protected float _explosionRadius = 7f;
    [SerializeField] protected float _explosionUpward = 0.4f;
    [SerializeField] private BoxCollider _firstPartCollider;
    [SerializeField] private BoxCollider _secondPartCollider;

    [Header("Only for volcano")]
    [SerializeField] private GameObject _particleSystem;

    private List<DestructibleObjectVoxel> _firstDestructibleVoxels = new List<DestructibleObjectVoxel>();
    private List<DestructibleObjectVoxel> _secondDestructibleVoxels = new List<DestructibleObjectVoxel>();
    private bool _isFirstPartDestroyed = false;

    private void Start()
    {
        _firstDestructibleVoxels = GetComponentsInChildren<DestructibleObjectVoxel>().ToList();

        foreach (DestructibleObjectVoxel voxel in _firstDestructibleVoxels)
        {
            if (voxel.gameObject.TryGetComponent<SecondPartVoxel>(out SecondPartVoxel secondPart))
            {
                _secondDestructibleVoxels.Add(voxel);
            }
        }
        foreach (DestructibleObjectVoxel voxel in _secondDestructibleVoxels)
        {
            _firstDestructibleVoxels.Remove(voxel);
        }
    }

    public void Collapse()
    {
        if(_isFirstPartDestroyed == false)
        {
            _firstPartCollider.enabled = false;
            ExplodeObject(_firstDestructibleVoxels.ToArray());
            _isFirstPartDestroyed = true;
            StartCoroutine(WaitForSecondPunch());

            if (_particleSystem != null)
            {
                Destroy(_particleSystem);
            }
        }
        else
        {
            _secondPartCollider.enabled = false;
            ExplodeObject(_secondDestructibleVoxels.ToArray());
        }
    }

    private void ExplodeObject(DestructibleObjectVoxel[] voxels)
    {
        foreach (DestructibleObjectVoxel voxel in voxels)
        {
            voxel.BoxCollider.isTrigger = false;
            voxel.Rigidbody.isKinematic = false;
            voxel.Rigidbody.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _explosionUpward);
            voxel.CollectVoxel();
        }
    }

    private IEnumerator WaitForSecondPunch()
    {
        if(_secondPartCollider != null)
        {
            yield return new WaitForSeconds(0.5f);
            _secondPartCollider.enabled = true;
        }
    }
}
