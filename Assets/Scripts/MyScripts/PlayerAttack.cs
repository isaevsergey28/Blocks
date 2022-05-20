using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MobAttack
{
    private void Start()
    {
        _parent = transform.parent.GetComponent<Player>();
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCollider(other);
    }

    public override void CollapseObject()
    {
        _currentDestructibleObject.Collapse();
        _currentDestructibleObject = null;
    }

    private void CheckCollider(Collider other)
    {
        if (other.TryGetComponent<SingleMeshDesctuctibleObject>(out SingleMeshDesctuctibleObject desctuctibleObjectMesh))
        {
            desctuctibleObjectMesh.ChangeObjects();
        }
        else if (other.TryGetComponent<DestructibleObject>(out DestructibleObject destructibleObject))
        {
            _currentDestructibleObject = destructibleObject;
            _parent.GetAnimStateSystem().ChangeState(State.Attack);
        }
        else if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            _opponent = enemy;
            HurtOpponent();
        }else if (other.TryGetComponent<HealingMob>(out HealingMob mob))
        {
            _opponent = mob;
            HurtHealingMob();
        }
    }

    private void HurtOpponent()
    {
        _parent.GetAnimStateSystem().ChangeState(State.Attack);
        _opponent.GetAnimStateSystem().StopAnimByName("Attack");
    }
    
    private void HurtHealingMob()
    {
        _parent.GetAnimStateSystem().ChangeState(State.Attack);
    }
}
