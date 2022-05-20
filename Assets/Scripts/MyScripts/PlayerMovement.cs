using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private JoystickInput _joystick;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private AnimationStateSystem _animStateSystem;

    public static Action onStartMove;

    private float _velocityMagnitude;
    private bool _isCanMove = true;

    private void Start()
    {
        BuildingRegistrar.onVictory += StopPlayer;
    }

    private void OnDisable()
    {
        BuildingRegistrar.onVictory -= StopPlayer;
    }

    private void FixedUpdate()
    {
        if(_isCanMove)
        {
            Move();
            UpdateRotation();
        }
    }

    public void StopPlayer()
    {
        _isCanMove = false;
    }

    public void AllowMovement()
    {
        _isCanMove = true;
    }

    private void UpdateRotation()
    {
        if (_joystick.HorizontalInput != 0 || _joystick.VerticalInput != 0)
        {
            onStartMove?.Invoke();
            if (_rigidbody.velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
            }
        }
    }

    private void Move()
    {
        _rigidbody.velocity = new Vector3(_joystick.HorizontalInput, 0f, _joystick.VerticalInput).normalized * _moveSpeed;
        _velocityMagnitude = _rigidbody.velocity.magnitude;
        SetMoveAnim();
    }

    private void SetMoveAnim()
    {
        _animStateSystem.ChangeState(State.StayOrMove);
        if (_velocityMagnitude == 5f)
        {
            _animStateSystem.StopAnimByName("Hurt");
        }
        _animStateSystem.SendAnimValue("MoveSpeed", _velocityMagnitude);
    }
}
