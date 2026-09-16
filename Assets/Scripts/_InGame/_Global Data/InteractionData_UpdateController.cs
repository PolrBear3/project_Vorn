using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionData_UpdateController : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Animator_Controller[] _healthUpdateAnimators;
    [SerializeField] private Animator_Controller[] _stateUpdateAnimators;


    private InteractionData _targetData;
    public InteractionData targetData => _targetData;

    private EventBus_Controller _healthUpdateActionBus = new();
    public EventBus_Controller healthUpdateActionBus => _healthUpdateActionBus;

    private EventBus_Controller _deathUpdateActionBus = new();
    public EventBus_Controller deathUpdateActionBus => _deathUpdateActionBus;

    public Action AfterDeathUpdate;


    // MonoBehaviour
    private void OnDestroy()
    {
        if (_targetData == null) return;

        _targetData.OnHealthModifyUpdate -= Handle_HealthUpdate;
        _targetData.OnStateUpdate -= Handle_StateUpdate;
    }


    // Data
    public void Set_Data(InteractionData targetData)
    {
        if (targetData == null) return;

        _targetData = targetData;
        Handle_StateUpdate();

        _targetData.OnHealthModifyUpdate += Handle_HealthUpdate;
        _targetData.OnStateUpdate += Handle_StateUpdate;
    }


    // Animator_Controller
    private void Play_AnimatorState(string animState)
    {
        for (int i = 0; i < _healthUpdateAnimators.Length; i++)
        {
            _healthUpdateAnimators[i].Play_State(animState);
        }
    }

    public bool CurrentAnimatorState_Playing()
    {
        for (int i = 0; i < _healthUpdateAnimators.Length; i++)
        {
            if (_healthUpdateAnimators[i].CurrentState_Playing()) return true;
        }
        return false;
    }


    // Health
    private void Handle_HealthUpdate(int healthModifyValue)
    {
        string animState = healthModifyValue <= 0 ? OccupantAnimation.Damaged : OccupantAnimation.Healed;
        Play_AnimatorState(animState);

        _targetData.Toggle_UpdatingState(true);
        StartCoroutine(HealthUpdate_Handle());
    }
    private IEnumerator HealthUpdate_Handle()
    {
        yield return null;
        while (CurrentAnimatorState_Playing()) yield return null;

        yield return _healthUpdateActionBus.RunSequential_DelayBusEvents();

        if (_targetData.currentHealth <= 0)
        {
            Play_AnimatorState(OccupantAnimation.Remove);

            yield return null;
            while (CurrentAnimatorState_Playing()) yield return null;

            yield return _deathUpdateActionBus.RunSequential_DelayBusEvents();
            AfterDeathUpdate?.Invoke();
        }

        _targetData.Toggle_UpdatingState(false);
        yield break;
    }


    // State
    private Animator_Controller Empty_StateUpdateAnimator()
    {
        for (int i = 0; i < _stateUpdateAnimators.Length; i++)
        {
            Animator_Controller animator = _stateUpdateAnimators[i];

            if (animator.currentState != null) continue;
            return animator;
        }
        return null;
    }
    private Animator_Controller StateUpdated_Animator(string updatedState)
    {
        for (int i = 0; i < _stateUpdateAnimators.Length; i++)
        {
            Animator_Controller animator = _stateUpdateAnimators[i];

            if (animator.currentState != updatedState) continue;
            return animator;
        }
        return null;
    }

    private void Handle_StateUpdate(InteractableState updatedState)
    {
        string updatedStateName = updatedState.ToString();
        bool stateAdded = _targetData.states.Contains(updatedState);

        Animator_Controller updateAnimator = stateAdded ? Empty_StateUpdateAnimator() : StateUpdated_Animator(updatedStateName);
        if (updateAnimator == null) return;

        if (stateAdded)
        {
            updateAnimator.Play_State(updatedStateName);
            return;
        }
        updateAnimator.StopCurrent_PlayingState();
    }
    private void Handle_StateUpdate()
    {
        if (_targetData == null) return;

        List<InteractableState> setStates = new(_targetData.states);

        foreach (InteractableState state in setStates)
        {
            Handle_StateUpdate(state);
        }
    }
}
