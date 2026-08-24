using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableAbility
{
    Taunt,
    Shield
}

public interface IInteractable
{
    InteractionData interactionData { get; }
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

    [Space(10)]
    [SerializeField] private List<InteractableAbility> _abilities = new();
    public List<InteractableAbility> abilities => _abilities;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;


    public Action<int> OnMaxHealthUpdate;
    public Action<int> OnCurrentHealthUpdate;
    public Action OnAbilityUpdate;


    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // New
    public InteractionData(InteractionData newData)
    {
        _mana = newData._mana;
        _maxHealth = newData._maxHealth;
        _currentHealth = _maxHealth;
        _previousCurrentHealth = _maxHealth;
        _healthModifyValue = newData._healthModifyValue;

        _abilities = new(newData.abilities);

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
        if (updateValue < 0 && Remove_Ability(InteractableAbility.Shield)) return;

        _currentHealth = newValue;
        OnCurrentHealthUpdate?.Invoke(updateValue);
    }

    public void Toggle_HealthUpdatingState(bool toggle)
    {
        _healthUpdating = toggle;
    }


    /// <returns> 
    /// true if add successful
    /// </returns>
    public bool Add_Ability(InteractableAbility updateAbility)
    {
        if (_abilities.Contains(updateAbility)) return false;

        _abilities.Add(updateAbility);
        OnAbilityUpdate?.Invoke();

        return true;
    }
    /// <returns> 
    /// true if remove successful
    /// </returns>
    public bool Remove_Ability(InteractableAbility removeAbility)
    {
        if (_abilities.Contains(removeAbility) == false) return false;

        _abilities.Remove(removeAbility);
        OnAbilityUpdate?.Invoke();

        return true;
    }
}