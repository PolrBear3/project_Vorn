using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _enemyPrefab;


    private EnemyManager_Data _data = new();
    public EnemyManager_Data data => _data;

    private List<Enemy> _spawnedEnemies = new();
    public List<Enemy> spawnedEnemies => _spawnedEnemies;

    public Action OnEnemyAction;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // Set_Data
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageSetEventBus.UnRegister(Spawn);
        stageManager.endTurnEventBus.UnRegister(Run_EnemyActions);
    }


    // Data
    private void Set_Data()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageSetEventBus.Register(0, Spawn);
        stageManager.endTurnEventBus.Register(1, Run_EnemyActions);
    }

    // Spawn
    private void Spawn(Enemy_ScrObj spawnEnemy, Tile spawnTile)
    {
        if (spawnEnemy == null || spawnTile == null) return;
        Vector2 spawnPos = (Vector2)spawnTile.transform.position + spawnEnemy.spawnOffset;

        GameObject enemyObj = Instantiate(_enemyPrefab, spawnPos, Quaternion.identity);
        enemyObj.transform.SetParent(transform);

        spawnTile.Set_Occupant(enemyObj);

        if (enemyObj.TryGetComponent(out Enemy spawnedEnemy) == false) return;
        _spawnedEnemies.Add(spawnedEnemy);

        spawnedEnemy.Set_Data(spawnEnemy);
        spawnedEnemy.movement.Set_CurrentTile(spawnTile);
    }

    private void Spawn(Enemy_SpawnData spawnData)
    {
        List<Enemy_ScrObj> spawnEnemies = spawnData.Spawn_Enemies();
        List<Tile> edgedSpawnTiles = GameManager.instance.tileManager.Edged_Tiles();

        for (int i = 0; i < spawnEnemies.Count; i++)
        {
            Tile spawnTile = edgedSpawnTiles[UnityEngine.Random.Range(0, edgedSpawnTiles.Count)];
            edgedSpawnTiles.Remove(spawnTile);

            Spawn(spawnEnemies[i], spawnTile);
        }
    }
    private void Spawn()
    {
        Spawn(GameManager.instance.stageManager.currentData.Update_EnemySpawnData());
    }


    // Spawned
    private void Run_EnemyActions() // change this to sequential animation delay ?
    {
        OnEnemyAction?.Invoke();
    }
}