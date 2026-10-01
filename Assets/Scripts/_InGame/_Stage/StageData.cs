using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    [ES3Serializable] private Stage_ScrObj _stage;
    public Stage_ScrObj stage => _stage;

    [ES3Serializable] private int _enemySpawnIndex;
    public int enemySpawnIndex => _enemySpawnIndex;

    [ES3Serializable] private bool _completed;
    public bool completed => _completed;


    // New
    public StageData(StageData loadStage)
    {
        _stage = loadStage._stage;
        _enemySpawnIndex = loadStage._enemySpawnIndex;
    }

    public StageData(Stage_ScrObj setStage)
    {
        _stage = setStage;
        _enemySpawnIndex = -1;
    }


    // Data
    /// <returns>
    /// Next index updated spawn data
    /// </returns>
    public Enemy_SpawnData Next_EnemySpawnData()
    {
        if (_stage is not BattleStage_ScrObj battleStage) return null;

        Enemy_SpawnData[] spawnDatas = battleStage.enemySpawnDatas;
        int updatedIndex = _enemySpawnIndex + 1;

        if (updatedIndex > spawnDatas.Length - 1) return null;
        return spawnDatas[updatedIndex];
    }

    /// <summary>
    /// Updates spawn index and
    /// </summary>
    /// <returns>
    /// Updated spawn data
    /// </returns>
    public Enemy_SpawnData Update_EnemySpawnData()
    {
        if (_stage is not BattleStage_ScrObj battleStage) return null;
        
        Enemy_SpawnData[] spawnDatas = battleStage.enemySpawnDatas;
        _enemySpawnIndex++;

        if (_enemySpawnIndex > spawnDatas.Length - 1) return null;
        return spawnDatas[_enemySpawnIndex];
    }


    public void Toggle_CompleteState(bool toggle)
    {
        _completed = toggle;
    }
}