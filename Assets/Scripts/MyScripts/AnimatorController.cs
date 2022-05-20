using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator _animator;

    private void Start()
    {
        if (!_animator)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    public void TurnOnAnim(State state)
    {
        switch (state)
        {
            case State.StayOrMove:
                _animator.SetBool("Move", true);
                break;
            case State.Attack:
                _animator.SetBool("Attack", true);
                break;
            case State.Build:
                _animator.SetBool("Build", true);
                break;
            case State.Victory:
                _animator.SetTrigger("Win");
                break;
            case State.Hurt:
                _animator.SetBool("Hurt", true);
                break;
            case State.Death:
                _animator.SetTrigger("Fall");
                break;
            case State.Revive:
                _animator.SetTrigger("Revive");
                break;
        }
    }

    public void DisableAnim(string name)
    {
        _animator.SetBool(name, false);
    }

    public void ChangeAnimValue(string animName, float value)
    {
        _animator.SetFloat(animName, value);
    }

    public Animator GetAnimator()
    {
        return _animator;
    }
}
