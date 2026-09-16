using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableState
{
    Taunt,
    Shield,
    Frozen
}

public interface IInteractable
{
    InteractionData interactionData { get; }
}

[System.Serializable]
public class InteractionData
{
    [SerializeField][Range(0, 100)] private int _maxHealth;
    public int maxHealth => _maxHealth;

    private int _currentHealth;
    public int currentHealth => _currentHealth;

    [SerializeField][Range(-100, 100)] private int _healthModifyValue;
    public int healthModifyValue => _healthModifyValue;

    [Space(10)]
    [SerializeField] private List<InteractableState> _states = new();
    public List<InteractableState> states => _states;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;


    /// <summary>
    /// currentHealth, maxHealth
    /// </summary>
    public Action<int, int> OnHealthUpdate;
    /// <summary>
    /// modified value
    /// </summary>
    public Action<int> OnHealthModifyUpdate;
    public Action<InteractableState> OnStateUpdate;

    private bool _dataUpdating;
    public bool dataUpdating => _dataUpdating;


    // New
    public InteractionData(InteractionData newData)
    {
        _maxHealth = newData._maxHealth;
        _currentHealth = _maxHealth;
        _healthModifyValue = newData._healthModifyValue;

        _states = new(newData.states);

        _interactRange = newData._interactRange;
        _targetSelectCount = newData._targetSelectCount;
    }


    // Data
    public void Update_MaxHealth(int newValue)
    {
        newValue = Mathf.Max(_currentHealth, newValue);
        int modifyValue = newValue - _maxHealth;

        if (modifyValue == 0) return;

        _maxHealth = newValue;

        OnHealthUpdate?.Invoke(_currentHealth, _maxHealth);
        OnHealthModifyUpdate?.Invoke(modifyValue);
    }
    public void Update_CurrentHealth(int newValue)
    {
        newValue = Mathf.Clamp(newValue, 0, _maxHealth);
        int modifyValue = newValue - _currentHealth;

        if (modifyValue == 0) return;
        if (modifyValue < 0 && Remove_State(InteractableState.Shield)) return;

        _currentHealth = newValue;

        OnHealthUpdate?.Invoke(_currentHealth, _maxHealth);
        OnHealthModifyUpdate?.Invoke(modifyValue);
    }

    public void Toggle_UpdatingState(bool toggle)
    {
        _dataUpdating = toggle;
    }


    /// <returns> 
    /// true if add successful
    /// </returns>
    public bool Add_State(InteractableState stateToUpdate)
    {
        if (_states.Contains(stateToUpdate)) return false;

        _states.Add(stateToUpdate);
        OnStateUpdate?.Invoke(stateToUpdate);

        return true;
    }
    /// <returns> 
    /// true if remove successful
    /// </returns>
    public bool Remove_State(InteractableState stateToRemove)
    {
        if (_states.Contains(stateToRemove) == false) return false;

        _states.Remove(stateToRemove);
        OnStateUpdate?.Invoke(stateToRemove);

        return true;
    }
}