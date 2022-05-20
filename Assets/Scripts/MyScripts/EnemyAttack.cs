using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MobAttack
{
    private void Start()
    {
        BuildingRegistrar.onVictory += DisableScript;

        _parent = transform.parent.GetComponent<Enemy>();
        _collider = GetComponent<BoxCollider>();
    }

    private void OnDisable()
    {
        BuildingRegistrar.onVictory -= DisableScript;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            _opponent = player;
            HurtOpponent();
        }
    }

    private void HurtOpponent()
    {
        _parent.GetAnimStateSystem().ChangeState(State.Attack);
    }

    private void DisableScript()
    {
        GetComponent<BoxCollider>().enabled = false;
    }
}
