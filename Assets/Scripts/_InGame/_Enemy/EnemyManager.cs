using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyManager_Data _data = new();
    public EnemyManager_Data data => _data;

    private List<Enemy> _spawnedEnemies = new();
    public List<Enemy> spawnedEnemies => _spawnedEnemies;

    private Coroutine _spawnCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageSetEventBus.UnRegister(Run_DelaySpawn);
        stageManager.endTurnEventBus.UnRegister(Run_EnemyActions);
    }


    // Data
    private void Set_Data()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageSetEventBus.Register(0, Run_DelaySpawn);
        stageManager.endTurnEventBus.Register(1, Run_EnemyActions);
    }


    // Spawn
    private Enemy Spawn(Enemy_ScrObj spawnEnemy, Tile spawnTile)
    {
        if (spawnEnemy == null || spawnTile == null) return null;
        Vector2 spawnPos = (Vector2)spawnTile.transform.position + spawnEnemy.spawnOffset;

        GameObject enemyObj = Instantiate(spawnEnemy.spawnPrefab, spawnPos, Quaternion.identity);
        enemyObj.transform.SetParent(transform);

        spawnTile.Set_Occupant(enemyObj);

        if (enemyObj.TryGetComponent(out Enemy spawnedEnemy) == false)
        {
            Destroy(enemyObj);
            return null;
        }
        _spawnedEnemies.Add(spawnedEnemy);

        spawnedEnemy.Set_Data(spawnEnemy);
        spawnedEnemy.movement.Set_CurrentTile(spawnTile);
        spawnedEnemy.animator.Play_State(OccupantAnimation.Set);

        return spawnedEnemy;
    }

    private IEnumerator DelaySpawn(Enemy_SpawnData spawnData)
    {
        List<Enemy_ScrObj> spawnEnemies = spawnData.Spawn_Enemies();
        List<Tile> edgedSpawnTiles = GameManager.instance.tileManager.Edged_Tiles();

        for (int i = 0; i < spawnEnemies.Count; i++)
        {
            Tile spawnTile = edgedSpawnTiles[UnityEngine.Random.Range(0, edgedSpawnTiles.Count)];
            edgedSpawnTiles.Remove(spawnTile);

            Enemy_ScrObj spawningEnemy = spawnEnemies[i];
            Enemy enemy = Spawn(spawningEnemy, spawnTile);

            Animator_Controller animator = enemy.animator;

            yield return null;
            while (enemy.animator.CurrentState_Playing()) yield return null;
        }

        _spawnCoroutine = null;
        yield break;
    }
    private IEnumerator Run_DelaySpawn()
    {
        Enemy_SpawnData spawnData = GameManager.instance.stageManager.currentData.Update_EnemySpawnData();
        if (spawnData == null) yield break;

        _spawnCoroutine = StartCoroutine(DelaySpawn(spawnData));

        while (_spawnCoroutine != null) yield return null;
        yield break;
    }


    // Spawned Enemies
    private IEnumerator Run_EnemyActions()
    {
        List<Enemy> actionEnemies = new(_spawnedEnemies);

        for (int i = 0; i < actionEnemies.Count; i++)
        {
            Enemy enemy = actionEnemies[i];
            if (enemy == null) continue;

            StartCoroutine(enemy.Run_EndTurnActions());
            while (enemy.actionsRunning) yield return null;
        }
        yield break;
    }
}