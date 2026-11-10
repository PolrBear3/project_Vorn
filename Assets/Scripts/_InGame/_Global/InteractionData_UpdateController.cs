using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionData_UpdateController : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Animator_Controller[] _healthUpdateAnimators;
    [SerializeField] private Animator_Controller[] _stateUpdateAnimators;

    [Space(20)]
    [SerializeField] private bool _preserveAfterDeath;


    private InteractionData _targetData;
    public InteractionData targetData => _targetData;

    private EventBus_Controller _healthUpdateActionBus = new();
    public EventBus_Controller healthUpdateActionBus => _healthUpdateActionBus;

    private EventBus_Controller _deathUpdateActionBus = new();
    public EventBus_Controller deathUpdateActionBus => _deathUpdateActionBus;

    public Action OnPreDeathActionDataUpdate;


    private bool _healthHandleUpdateRunning;
    public bool healthHandleUpdateRunning => _healthHandleUpdateRunning;


    // Data
    public void Set_Data(InteractionData targetData)
    {
        if (targetData == null) return;
        _targetData = targetData;
    }


    // Animator_Controller
    public bool CurrentAnimatorState_Playing()
    {
        for (int i = 0; i < _healthUpdateAnimators.Length; i++)
        {
            if (_healthUpdateAnimators[i].CurrentState_Playing()) return true;
        }
        return false;
    }
    private void Play_AnimatorState(string animState)
    {
        if (animState == null) return;

        for (int i = 0; i < _healthUpdateAnimators.Length; i++)
        {
            _healthUpdateAnimators[i].Play_State(animState);
        }
    }


    // Health
    public IEnumerator Handle_HealthUpdate(int healthModifyValue)
    {
        _healthHandleUpdateRunning = true;

        int currentHealth = _targetData.currentHealth;
        Play_AnimatorState(currentHealth > 0 && healthModifyValue > 0 ? OccupantAnimation.Healed : OccupantAnimation.Damaged);

        yield return null;
        while (CurrentAnimatorState_Playing()) yield return null;

        yield return _healthUpdateActionBus.RunSequential_DelayBusEvents();

        if (_targetData.currentHealth > 0)
        {
            _healthHandleUpdateRunning = false;
            yield break;
        }

        Play_AnimatorState(OccupantAnimation.Remove);

        yield return null;
        while (CurrentAnimatorState_Playing()) yield return null;

        OnPreDeathActionDataUpdate?.Invoke();
        yield return _deathUpdateActionBus.RunSequential_DelayBusEvents();

        if (_preserveAfterDeath)
        {
            _healthHandleUpdateRunning = false;
            yield break;
        }
        Destroy(gameObject);
    }


    // State
    private Animator_Controller Empty_StateUpdateAnimator()
    {
        int currentStateCount = _targetData.states.Count;
        if (currentStateCount >= _stateUpdateAnimators.Length) return null;

        return _stateUpdateAnimators[currentStateCount];
    }
    private Animator_Controller StateUpdated_Animator(InteractableState updatedState)
    {
        for (int i = 0; i < _stateUpdateAnimators.Length; i++)
        {
            Animator_Controller animator = _stateUpdateAnimators[i];
            string stateName = InteractableState_Animation.InteractableState_AnimationName(updatedState, true);

            if (animator.currentState != stateName) continue;
            return animator;
        }
        return null;
    }

    public void Handle_StateUpdate(InteractableState updatedState)
    {
        bool stateAdded = _targetData.states.Contains(updatedState);

        Animator_Controller updateAnimator = stateAdded ? Empty_StateUpdateAnimator() : StateUpdated_Animator(updatedState);
        if (updateAnimator == null) return;

        string updateStateName = InteractableState_Animation.InteractableState_AnimationName(updatedState, stateAdded);
        updateAnimator.Play_State(updateStateName);
    }
    public void Handle_StateUpdate()
    {
        if (_targetData == null) return;

        List<InteractableState> setStates = new(_targetData.states);

        foreach (InteractableState state in setStates)
        {
            Handle_StateUpdate(state);
        }
    }
}