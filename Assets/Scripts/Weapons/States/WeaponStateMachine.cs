using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System;
public class WeaponStateMachine
{
    private WeaponState _currentState;
    private Dictionary<Type, WeaponState> _states = new Dictionary<Type, WeaponState>();
    
    private WeaponState _idleState;
    private WeaponState _firingState;
    private WeaponState _cooldownState;
    private WeaponState _reloadingState;
    private WeaponState _chargeState;

    public float BurstStartTime { get; set; }
    public float CooldownEndTime { get; set; }
    public float? ResumeTime { get; set; }


    public WeaponStateMachine(Weapon weapon)
    {
        _idleState = new WeaponIdleState(weapon);
        _firingState = new WeaponFiringState(weapon);
        _cooldownState = new WeaponCooldownState(weapon);
        _reloadingState = new WeaponReloadingState(weapon);
        _chargeState = new WeaponChargeState(weapon);

        _states = new Dictionary<Type, WeaponState>
        {
            { typeof(WeaponIdleState), _idleState },
            { typeof(WeaponFiringState), _firingState },
            { typeof(WeaponCooldownState), _cooldownState },
            { typeof(WeaponReloadingState), _reloadingState },
            { typeof(WeaponChargeState), _chargeState }
        };

    }

    public void Update(bool isPressed)
    {
        WeaponState state;
        do
        {
            state = _currentState;
            state?.Update(isPressed);
        }
        while (_currentState != state && (_currentState == _firingState || _currentState == _cooldownState));
    }

    public void OnFire()
    {
        _currentState?.OnFire();
    }

    public void OnReload()
    {
        _currentState?.OnReload();
    }

    public void ChangeState<T>() where T : WeaponState
    {
        ChangeState(_states[typeof(T)]);
    }

    private void ChangeState(WeaponState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}
