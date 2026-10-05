using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    [ES3Serializable] private Stage_ScrObj _stage;
    public Stage_ScrObj stage => _stage;

    [ES3Serializable] private bool _completed;
    public bool completed => _completed;

    [ES3Serializable] private int _enemySpawnIndex;
    public int enemySpawnIndex => _enemySpawnIndex;

    [ES3Serializable] private List<Event_ScrObj> _queueEvents = new();
    public List<Event_ScrObj> queueEvents => _queueEvents;


    // New
    public StageData(StageData loadStage)
    {
        _stage = loadStage._stage;
    }

    public StageData(Stage_ScrObj setStage)
    {
        _stage = setStage;
    }


    // Data
    public void Toggle_CompleteState(bool toggle)
    {
        _completed = toggle;
    }


    // Battle Stage
    public Enemy_SpawnData Current_EnemySpawnData()
    {
        if (_stage is not BattleStage_ScrObj battleStage) return null;

        Enemy_SpawnData[] spawnDatas = battleStage.enemySpawnDatas;
        if (_enemySpawnIndex < 0 || _enemySpawnIndex >= spawnDatas.Length) return null;

        return spawnDatas[_enemySpawnIndex];
    }
    public bool Update_EnemySpawnData()
    {
        if (_stage == null) return false;
        if (_stage is not BattleStage_ScrObj battleStage) return false;

        if (_enemySpawnIndex >= battleStage.enemySpawnDatas.Length - 1) return false;

        _enemySpawnIndex++;
        return true;
    }


    // Event Stage
    public bool Update_QueueEvents()
    {
        if (_stage == null) return false;
        if (_stage is not EventStage_ScrObj eventStage) return false;

        if (_queueEvents != null && _queueEvents.Count > 0) return true;

        _queueEvents = eventStage.Combined_Events();
        return true;
    }
}