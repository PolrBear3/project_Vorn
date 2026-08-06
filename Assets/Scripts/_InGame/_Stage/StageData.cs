using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    private Stage_ScrObj _stage;
    public Stage_ScrObj stage => _stage;

    private int _enemySpawnIndex;
    public int enemySpawnIndex => _enemySpawnIndex;


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
    public Enemy_SpawnData Update_EnemySpawnData()
    {
        Enemy_SpawnData[] spawnDatas = _stage.enemySpawnDatas;
        _enemySpawnIndex++;

        if (_enemySpawnIndex > spawnDatas.Length - 1) return null;
        return spawnDatas[_enemySpawnIndex];
    }
}