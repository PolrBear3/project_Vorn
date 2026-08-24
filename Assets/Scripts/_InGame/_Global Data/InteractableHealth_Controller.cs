using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableHealth_Controller : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Animator_Controller[] _healthUpdateAnimators;


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
        _targetData.OnCurrentHealthUpdate -= Handle_HealthUpdate;
    }
    

    // Data
    public void Set_Data(InteractionData targetData)
    {
        if (targetData == null) return;
        
        _targetData = targetData;
        _targetData.OnCurrentHealthUpdate += Handle_HealthUpdate;
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


    // Main
    private void Handle_HealthUpdate(int healthUpdateValue)
    {
        string animState = healthUpdateValue <= 0 ? OccupantAnimation.Damaged : OccupantAnimation.Healed;
        Play_AnimatorState(animState);

        _targetData.Toggle_HealthUpdatingState(true);
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

        _targetData.Toggle_HealthUpdatingState(false);
        yield break;
    }
}
