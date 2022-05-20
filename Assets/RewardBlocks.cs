using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardBlocks : MonoBehaviour
{
    private Rigidbody _rigidbody = null;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
    
}
