using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private AnimationStateSystem _animStateSystem;

    private GameObject _target;
    private NavMeshAgent _agent;
    private bool _isCanMove = true;
    [SerializeField] private float moveSpeed = 0f;

    private void Start()
    {
        _target = FindObjectOfType<PlayerMovement>().gameObject;
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        CheckDistance();
        if (_isCanMove)
        {
            MoveToTarget();
            SetMoveAnim();
        }
        else
        {
            _animStateSystem.SendAnimValue("MoveSpeed", 0f);
        }
    }

    public void CheckDistance()
    {
        Vector3 direction = _target.transform.position - transform.position;
        float distance = direction.magnitude;
        if (distance < 1.5f || _animStateSystem.CheckForAnimActive("Attack"))
        {
            _isCanMove = false;
            _agent.isStopped = true;
        }
        else
        {
            _isCanMove = true;
            _agent.isStopped = false;
        }
    }

    public void Stop()
    {
        _agent.isStopped = true;
        _animStateSystem.SendAnimValue("MoveSpeed", 0f);
    }

    public void AllowMovement()
    {
        _agent.isStopped = false;
        _animStateSystem.SendAnimValue("MoveSpeed", 5f);
        _agent.speed = moveSpeed;
    }

    private void SetMoveAnim()
    {
        _animStateSystem.ChangeState(State.StayOrMove);
        _animStateSystem.SendAnimValue("MoveSpeed", 5f);
        _agent.speed = moveSpeed;
    }

    private void MoveToTarget()
    {
        Vector3 destination = _target.transform.position - _agent.transform.position;
        _agent.SetDestination(_target.transform.position);
    }
}
 