using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    InteractionData interactionData { get; }
    bool healthUpdating { get; }
}

[System.Serializable]
public class InteractionData
{
    [SerializeField][Range(0, 100)] private int _mana;
    public int mana => _mana;

    [SerializeField][Range(0, 100)] private int _maxHealth;
    public int maxHealth => _maxHealth;

    private int _currentHealth;
    public int currentHealth => _currentHealth;

    private int _previousCurrentHealth;
    public int previousCurrentHealth => _previousCurrentHealth;

    [SerializeField][Range(-100, 100)] private int _healthModifyValue;
    public int healthModifyValue => _healthModifyValue;


    public Action<int> OnMaxHealthUpdate;
    public Action<int> OnCurrentHealthUpdate;


    [Space(10)]
    [SerializeField][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;


    // New
    public InteractionData(InteractionData newData)
    {
        _mana = newData._mana;
        _maxHealth = newData._maxHealth;
        _currentHealth = _maxHealth;
        _previousCurrentHealth = _maxHealth;
        _healthModifyValue = newData._healthModifyValue;

        _interactRange = newData._interactRange;
        _targetSelectCount = newData._targetSelectCount;
    }


    // Data
    public void Update_MaxHealth(int newValue)
    {
        newValue = Mathf.Max(_currentHealth, newValue);

        int updateValue = newValue - _maxHealth;
        if (updateValue == 0) return;

        _maxHealth = newValue;
        OnMaxHealthUpdate?.Invoke(updateValue);
    }

    public void Update_CurrentHealth(int newValue)
    {
        _previousCurrentHealth = _currentHealth;
        newValue = Mathf.Clamp(newValue, 0, _maxHealth);

        int updateValue = newValue - _currentHealth;
        if (updateValue == 0) return;

        _currentHealth = newValue;
        OnCurrentHealthUpdate?.Invoke(updateValue);
    }
}