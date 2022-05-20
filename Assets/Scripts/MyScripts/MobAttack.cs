using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAttack : MonoBehaviour
{
    [SerializeField] protected int _damage;

    protected BoxCollider _collider;
    protected Mob _opponent;
    protected Mob _parent;
    protected DestructibleObject _currentDestructibleObject;

    public void MakePunch()
    {
        _opponent.GiveDamage(_damage);
    }

    public void NullyfyOpponent()
    {
        _opponent = null;
    }

    public Mob GetParent()
    {
        return _parent;
    } 
    
    public Mob GetOpponent()
    {
        return _opponent;
    }

    public bool IsOpponentLinkActive()
    {
        return _opponent;
    }

    public bool IsDestructibleObjectLinkActive()
    {
        return _currentDestructibleObject;
    }

    public virtual void CollapseObject() { }

    public float CalculateDistance()
    {
        Vector3 direction = transform.position - _opponent.transform.position;
        float distance = direction.magnitude;
        return distance;
    }
}
