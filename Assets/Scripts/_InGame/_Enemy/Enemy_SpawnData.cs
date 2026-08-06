using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy_SpawnRateData
{
    [SerializeField] private Enemy_ScrObj _spawnEnemy;
    public Enemy_ScrObj spawnEnemy => _spawnEnemy;

    [SerializeField][Range(0, 100)] private int _spawnRate;
    public int spawnRate => _spawnRate;
}

[System.Serializable]
public class Enemy_SpawnData
{
    [SerializeField][Range(0, 100)] private int _spawnCount;
    public int spawnCount => _spawnCount;

    [SerializeField] private Enemy_SpawnRateData[] _spawnRateDatas;
    public Enemy_SpawnRateData[] spawnRateDatas => _spawnRateDatas;


    // Data
    private int SpawnRates_TotalValue()
    {
        int totalValue = 0;
        if (_spawnRateDatas.Length <= 0) return totalValue;

        for (int i = 0; i < _spawnRateDatas.Length; i++)
        {
            totalValue += _spawnRateDatas[i].spawnRate;
        }
        return totalValue;
    }
    private Enemy_ScrObj RateRandom_SpawnEnemy()
    {
        if (_spawnRateDatas.Length <= 0) return null;

        // get total wieght
        int totalWeight = SpawnRates_TotalValue();
        if (totalWeight <= 0) return _spawnRateDatas[Random.Range(0, _spawnRateDatas.Length)].spawnEnemy;

        // track values
        int randValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // get random according to weight
        for (int i = 0; i < _spawnRateDatas.Length; i++)
        {
            Enemy_SpawnRateData spawnRateData = _spawnRateDatas[i];
            cumulativeWeight += spawnRateData.spawnRate;

            if (randValue >= cumulativeWeight) continue;
            return spawnRateData.spawnEnemy;
        }
        return null;
    }

    public List<Enemy_ScrObj> Spawn_Enemies()
    {
        List<Enemy_ScrObj> spawnEnemies = new();

        for (int i = 0; i < _spawnCount; i++)
        {
            spawnEnemies.Add(RateRandom_SpawnEnemy());
        }
        return spawnEnemies;
    }
}