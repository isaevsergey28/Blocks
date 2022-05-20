using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class DestructibleObjectVoxel : MonoBehaviour
{
    public static event Action onSuckingCube;
    public Rigidbody Rigidbody { get; private set; }
    public BoxCollider BoxCollider { get; private set; }

    private bool _isCollectabe = false;
   

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        BoxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Hoover>(out Hoover hoover) && _isCollectabe)
        {
            _isCollectabe = false;
            Rigidbody.isKinematic = true;
            BoxCollider.isTrigger = true;
            StartCoroutine(MoveToTarget(gameObject.transform, hoover.gameObject));
        }
    }
    public void CollectVoxel()
    {
        StartCoroutine(WaitForCollect());
    }

    private IEnumerator MoveToTarget(Transform voxel, GameObject target)
    {
        Destroy(Rigidbody);
        Destroy(BoxCollider);

        while (SqrMagnitude(voxel.gameObject, target) > 0.1f)
        {
            voxel.position = Vector3.MoveTowards(voxel.position, target.transform.position, Time.deltaTime * 10f);
            yield return new WaitForSeconds(0.01f);
        }
        onSuckingCube?.Invoke();
        Destroy(this.gameObject);
    }

    private IEnumerator WaitForCollect()
    {
        yield return new WaitForSeconds(0.4f);
        _isCollectabe = true;
        BoxCollider.enabled = false;
        BoxCollider.enabled = true;
    }

    private float SqrMagnitude(GameObject voxel, GameObject target)
    {
        Vector2 offset = target.transform.position - voxel.transform.position;
        return offset.sqrMagnitude;
    }
}
