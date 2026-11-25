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
    InteractionData_UpdateController interactionDataUpdater { get; }
}

[System.Serializable]
public class InteractionData
{
    [SerializeField][ES3Serializable][Range(0, 100)] private int _maxHealth;
    public int maxHealth => _maxHealth;

    [ES3Serializable] private int _currentHealth;
    public int currentHealth => _currentHealth;

    [ES3Serializable] private List<int> _healthModifiedHistoryDatas = new();


    [SerializeField] private InteractableState[] _setStates;

    /// <summary>
    /// added state & end turn count
    /// </summary>
    [ES3Serializable] private Dictionary<InteractableState, int> _stateDatas = new();
    public Dictionary<InteractableState, int> stateDatas => _stateDatas;


    [Space(10)]
    [SerializeField][ES3Serializable][Range(-100, 100)] private int _healthModifyValue;
    public int healthModifyValue => _healthModifyValue;

    [ES3Serializable] private List<int> _healthModifyHistoryDatas = new();
    public List<int> healthModifyHistoryDatas => _healthModifyHistoryDatas;


    [Space(10)]
    [SerializeField][ES3Serializable][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][ES3Serializable][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;


    private const int _maxHistoryDataCount = 10;


    /// <summary>
    /// maxHealth, currentHealth
    /// </summary>
    public Action<int, int> OnHealthUpdate;
    /// <summary>
    /// modified value
    /// </summary>
    public Action<int> OnHealthModifiedUpdate;
    /// <summary>
    /// udpated state
    /// </summary>
    public Action<InteractableState> OnStateUpdate;


    // New
    public InteractionData(InteractionData newData)
    {
        _maxHealth = newData._maxHealth;
        _currentHealth = _maxHealth;
        _healthModifiedHistoryDatas = new(newData._healthModifiedHistoryDatas);

        _setStates = newData._setStates;

        for (int i = 0; i < _setStates.Length; i++)
        {
            Add_State(_setStates[i]);
        }

        _healthModifyValue = newData._healthModifyValue;
        _healthModifyHistoryDatas = new(newData._healthModifyHistoryDatas);

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


    public int HealthModifiedHistory_DataValue(int dataIndex)
    {
        if (_healthModifiedHistoryDatas == null) return 0;

        int dataCount = _healthModifiedHistoryDatas.Count;
        if (dataCount <= 0) return 0;

        int index = Mathf.Clamp(dataIndex, 0, dataCount - 1);
        return _healthModifiedHistoryDatas[dataCount - 1 - index];
    }
    private void Track_HealthModifiedHistory(int modifyValue)
    {
        if (modifyValue == 0) return;
        _healthModifiedHistoryDatas.Add(modifyValue);

        if (_healthModifiedHistoryDatas.Count <= _maxHistoryDataCount) return;
        _healthModifiedHistoryDatas.RemoveAt(0);
    }

    public bool Update_MaxHealth(int newValue, out int modifiedValue)
    {
        newValue = Mathf.Max(_currentHealth, newValue);
        modifiedValue = newValue - _maxHealth;

        if (modifiedValue == 0 && newValue > 0) return false;

        _maxHealth = newValue;

        OnHealthModifiedUpdate?.Invoke(modifiedValue);
        OnHealthUpdate?.Invoke(_maxHealth, _currentHealth);

        return true;
    }
    public bool Update_CurrentHealth(int newValue, out int modifiedValue)
    {
        newValue = Mathf.Min(newValue, _maxHealth);
        modifiedValue = newValue - _currentHealth;

        if (modifiedValue == 0 && newValue > 0) return false;

        if (_currentHealth > 0 && modifiedValue < 0 && Remove_State(InteractableState.Shield))
        {
            modifiedValue = 0;
            return false;
        }

        _currentHealth = newValue;
        Track_HealthModifiedHistory(modifiedValue);

        OnHealthModifiedUpdate?.Invoke(modifiedValue);
        OnHealthUpdate?.Invoke(_maxHealth, _currentHealth);

        return true;
    }


    public int HealthModifyHistory_DataValue(int dataIndex)
    {
        if (_healthModifyHistoryDatas == null) return 0;

        int dataCount = _healthModifyHistoryDatas.Count;
        if (dataCount <= 0) return 0;

        int index = Mathf.Clamp(dataIndex, 0, dataCount - 1);
        return _healthModifyHistoryDatas[dataCount - 1 - index];
    }
    public void Track_HealthModifyHistory(int modifyValue)
    {
        if (modifyValue == 0) return;
        _healthModifyHistoryDatas.Add(modifyValue);

        if (_healthModifyHistoryDatas.Count <= _maxHistoryDataCount) return;
        _healthModifyHistoryDatas.RemoveAt(0);
    }


    /// <returns> 
    /// true if add successful
    /// </returns>
    public bool Add_State(InteractableState stateToUpdate)
    {
        int endTurnCount = GameManager.instance.stageManager.endTurnCount;

        if (_stateDatas.ContainsKey(stateToUpdate))
        {
            _stateDatas[stateToUpdate] = endTurnCount;
            return false;
        }

        _stateDatas.Add(stateToUpdate, endTurnCount);
        OnStateUpdate?.Invoke(stateToUpdate);

        return true;
    }
    /// <returns> 
    /// true if remove successful
    /// </returns>
    public bool Remove_State(InteractableState stateToRemove)
    {
        if (_stateDatas.ContainsKey(stateToRemove) == false) return false;

        _stateDatas.Remove(stateToRemove);
        OnStateUpdate?.Invoke(stateToRemove);

        return true;
    }

    public bool Has_State(InteractableState stateToSearch)
    {
        return _stateDatas.ContainsKey(stateToSearch);
    }
    public bool Has_State(InteractableState stateToSearch, out int addedTurnCount)
    {
        addedTurnCount = 0;
        if (Has_State(stateToSearch) == false) return false;

        addedTurnCount = _stateDatas[stateToSearch];
        return true;
    }
}
