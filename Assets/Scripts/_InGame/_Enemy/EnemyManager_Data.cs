using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager_Data
{
    private Dictionary<EnemyData, Vector2> _spawnedEnemyDatas = new();
    public Dictionary<EnemyData, Vector2> spawnedEnemyDatas => _spawnedEnemyDatas;


    // Data
    public void Save_SpawnedEnemyDatas(List<Enemy> spawnedEnemies)
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            Enemy enemy = spawnedEnemies[i];
            _spawnedEnemyDatas.Add(enemy.data, enemy.movement.currentTile.data.position);
        }
    }
}
