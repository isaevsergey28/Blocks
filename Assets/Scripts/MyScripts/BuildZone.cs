using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public static event Action<FinalObject> onTriggerEnterBuildZoneEvent;
    public static event Action onTriggerExitBuildZoneEvent;

    private AnimationStateSystem _playerAnimationStateSystem;
    private Backpack _backpack;
    private bool _isPlayerOnBuildZone = false;
    private FinalCamera _finalCamera;

    private void Start()
    {
        _finalCamera = GetComponentInChildren<FinalCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AnimationStateSystem>(out AnimationStateSystem animationStateSystem) &&
            other.TryGetComponent<Player>(out Player player))
        {
            _isPlayerOnBuildZone = true;
            _playerAnimationStateSystem = animationStateSystem;
            _backpack = animationStateSystem.GetComponentInChildren<Backpack>();

            if (_backpack)
            {
                if(_backpack.GetResourceAmount() > 0)
                {
                    animationStateSystem.ChangeState(State.Build);
                    _finalCamera.ActiveFinalCamera();
                    onTriggerEnterBuildZoneEvent?.Invoke(GetComponentInChildren<FinalObject>());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<AnimationStateSystem>(out AnimationStateSystem animationStateSystem) &&
             other.TryGetComponent<Player>(out Player player))
        {
            _isPlayerOnBuildZone = false;
            onTriggerExitBuildZoneEvent?.Invoke();
            StopBuilding();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(_isPlayerOnBuildZone)
        {
            if (_backpack.GetResourceAmount() == 0)
            {
                StopBuilding();
            }
        }
    }

    public void StopBuilding()
    {
        _finalCamera.InactiveFinalCamera();
        _playerAnimationStateSystem.StopAnimByName("Build");
        _playerAnimationStateSystem.ChangeState(State.StayOrMove);
    }
}
