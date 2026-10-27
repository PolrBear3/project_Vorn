using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill_EnemySpawner : EnemySkill
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _spawnCount;
    [SerializeField] private Enemy_ScrObj[] _spawnEnemies;


    // Data
    private Enemy_ScrObj SpawnEnemy()
    {
        int spawnEnemyCount = _spawnEnemies.Length;

        if (spawnEnemyCount <= 0) return null;
        return _spawnEnemies[Random.Range(0, spawnEnemyCount)];
    }


    // EnemySkill
    public override IEnumerator Trigger_Skill()
    {
        EnemyManager enemyManager = GameManager.instance.enemyManager;
        List<Tile> spawnTiles = SpawnTarget_Tiles(_spawnCount);

        for (int i = 0; i < spawnTiles.Count; i++)
        {
            yield return enemyManager.DelaySpawn(SpawnEnemy(), spawnTiles[i]);
        }
    }
}