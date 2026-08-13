using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    InteractionData interactionData { get; }
}

[System.Serializable]
public class InteractionData
{
    [SerializeField][Range(0, 10)] private int _mana;
    public int mana => _mana;

    [SerializeField][Range(0, 100)] private int _health;
    public int health => _health;

    [SerializeField][Range(-100, 100)] private int _healthModifyValue;
    public int healthModifyValue => _healthModifyValue;


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
        _health = newData._health;
        _healthModifyValue = newData._healthModifyValue;

        _interactRange = newData._interactRange;
        _targetSelectCount = newData._targetSelectCount;
    }


    // Data
    public void Update_CurrentHealth(int newValue)
    {
        newValue = Mathf.Max(0, newValue);
        int updateValue = newValue - _health;

        _health = newValue;
        OnCurrentHealthUpdate?.Invoke(updateValue);
    }
}