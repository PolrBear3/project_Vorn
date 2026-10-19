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
    [SerializeField][ES3Serializable][Range(0, 100)] private int _maxHealth;
    public int maxHealth => _maxHealth;

    [ES3Serializable] private int _currentHealth;
    public int currentHealth => _currentHealth;

    [SerializeField][ES3Serializable] private List<InteractableState> _states = new();
    public List<InteractableState> states => _states;


    [Space(10)]
    [SerializeField][ES3Serializable][Range(-100, 100)] private int _healthModifyValue;
    public int healthModifyValue => _healthModifyValue;

    [ES3Serializable] private int _healthModifySuccessCount;
    public int healthModifySuccessCount => _healthModifySuccessCount;


    [Space(10)]
    [SerializeField][ES3Serializable][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][ES3Serializable][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;


    /// <summary>
    /// currentHealth, maxHealth
    /// </summary>
    public Action<int, int> OnHealthUpdate;
    /// <summary>
    /// modified value
    /// </summary>
    public Action<int> OnHealthModifiedUpdate;
    public Action<InteractableState> OnStateUpdate;

    private bool _dataUpdating;
    public bool dataUpdating => _dataUpdating;


    // New
    public InteractionData(InteractionData newData)
    {
        _maxHealth = newData._maxHealth;
        _currentHealth = _maxHealth;

        _states = new(newData.states);

        _healthModifyValue = newData._healthModifyValue;
        _healthModifySuccessCount = newData._healthModifySuccessCount;

        _interactRange = newData._interactRange;
        _targetSelectCount = newData._targetSelectCount;
    }


    // Data
    public Dictionary<string, object> DescriptionFormats()
    {
        Dictionary<string, object> datas = new()
        {
            { "healOrDamage", _healthModifyValue > 0 ? "heal" : "damage"},
            { "healthModifyValue", (_healthModifyValue > 0 ? "+" : "") + _healthModifyValue },
            { "interactRange", _interactRange },
            { "targetSelectCount", _targetSelectCount }
        };
        return datas;
    }


    public bool Update_MaxHealth(int newValue)
    {
        newValue = Mathf.Max(_currentHealth, newValue);
        int modifyValue = newValue - _maxHealth;

        if (modifyValue == 0 && newValue > 0) return false;

        _maxHealth = newValue;

        OnHealthUpdate?.Invoke(_currentHealth, _maxHealth);
        OnHealthModifiedUpdate?.Invoke(modifyValue);

        return true;
    }
    public bool Update_CurrentHealth(int newValue)
    {
        newValue = Mathf.Min(newValue, _maxHealth);

        int modifyValue = newValue - _currentHealth;
        if (modifyValue == 0 && newValue > 0) return false;

        OnHealthModifiedUpdate?.Invoke(modifyValue);
        if (modifyValue < 0 && Remove_State(InteractableState.Shield)) return false;

        _currentHealth = newValue;
        OnHealthUpdate?.Invoke(_currentHealth, _maxHealth);

        return true;
    }

    public bool Modify_OtherData_CurrentHealth(InteractionData otherData, int modifyValue)
    {
        if (otherData == null || otherData == this) return false;
        if (otherData.Update_CurrentHealth(otherData.currentHealth + modifyValue) == false) return false;

        _healthModifySuccessCount++;
        return true;
    }
    public bool Modify_OtherData_CurrentHealth(InteractionData otherData)
    {
        return Modify_OtherData_CurrentHealth(otherData, _healthModifyValue);
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


    public void Toggle_DataUpdate(bool toggle)
    {
        _dataUpdating = toggle;
    }
}