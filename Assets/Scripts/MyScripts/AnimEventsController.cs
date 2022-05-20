using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventsController : MonoBehaviour
{
    [SerializeField] private GameObject _attackTriggerObject;

    private MobAttack _mobAttack;

    private void Start()
    {
        _mobAttack = _attackTriggerObject.GetComponent<MobAttack>();
    }

    public void SignalAttackEnd()
    {
        if (_mobAttack.IsOpponentLinkActive())
        {
            float distance = _mobAttack.CalculateDistance();
            if (distance < 1.5f)
            {
                _mobAttack.GetOpponent().GetAnimStateSystem().ChangeState(State.Hurt);
                _mobAttack.MakePunch();
                _mobAttack.GetOpponent().PlayShashParticles();
            }
            else
            {
                _mobAttack.GetOpponent().GetAnimStateSystem().StopAnimByName("Hurt");
            }
            _mobAttack.NullyfyOpponent();
        }
        else if (_mobAttack.IsDestructibleObjectLinkActive())
        {
            _mobAttack.CollapseObject();
        }
        _mobAttack.GetParent().GetAnimStateSystem().StopAnimByName("Attack");
    }

    public void StopHurtAnim()
    {
        _mobAttack.GetParent().GetAnimStateSystem().StopAnimByName("Hurt");
    }
}
