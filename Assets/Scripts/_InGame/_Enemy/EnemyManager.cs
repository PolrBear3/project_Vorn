using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, ISubscriptionHandler
{
    private EnemyManager_Data _data = new();  // for save & load
    public EnemyManager_Data data => _data;

    private List<Enemy> _spawnedEnemies = new();
    public List<Enemy> spawnedEnemies => _spawnedEnemies;


    [Space(20)]
    [SerializeField] private ToolTip _enemyHoverToolTip;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller endTurnBus = stageManager.endTurnEventBus;

        stageManager.setStageEventBus.Register(1, Spawn_CurrentSpawnData);

        endTurnBus.Register(3, Run_EnemyActions);
        endTurnBus.Register(3, Update_SpawnData);
        endTurnBus.Register(3, Spawn_CurrentSpawnData);

        endTurnBus.Register(StageEnemies_Cleared);
        stageManager.endStageEventBus.Register(StageEnemies_NotCleared);

        manager.tileManager.tileHoverEventBus.Register(0, Hover_Enemy);
        endTurnBus.OnSequentialDelayFinish += Hover_Enemy;

        endTurnBus.Register(0, _enemyHoverToolTip.UnToggle);
    }

    public void UnSubscribe_All()
    {
        // from Set_Data
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller endTurnBus = stageManager.endTurnEventBus;

        stageManager.setStageEventBus.UnRegister(Spawn_CurrentSpawnData);

        endTurnBus.UnRegister(Run_EnemyActions);
        endTurnBus.UnRegister(Update_SpawnData);
        endTurnBus.UnRegister(Spawn_CurrentSpawnData);

        endTurnBus.UnRegister(StageEnemies_Cleared);
        stageManager.endStageEventBus.UnRegister(StageEnemies_NotCleared);

        manager.tileManager.tileHoverEventBus.UnRegister(Hover_Enemy);
        endTurnBus.OnSequentialDelayFinish -= Hover_Enemy;

        endTurnBus.UnRegister(_enemyHoverToolTip.UnToggle);
    }


    // Data
    private Enemy Spawned_Enemy(Tile targetTile)
    {
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            Enemy enemy = _spawnedEnemies[i];

            if (enemy.movement.currentTile != targetTile) continue;
            return enemy;
        }
        return null;
    }

    public bool StageEnemies_NotCleared()
    {
        StageData currentStageData = GameManager.instance.stageManager.stageMap.data.Current_StageData();

        return currentStageData != null && _spawnedEnemies.Count > 0;
    }
    private bool StageEnemies_Cleared()
    {
        return StageEnemies_NotCleared() == false;
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
    private IEnumerator Sequential_DelaySpawn(Enemy_SpawnData spawnData)
    {
        List<Enemy_ScrObj> spawnEnemies = spawnData.Spawn_Enemies();
        List<Tile> edgedSpawnTiles = GameManager.instance.tileManager.Edged_Tiles();

        for (int i = 0; i < spawnEnemies.Count; i++)
        {
            Tile spawnTile = edgedSpawnTiles[UnityEngine.Random.Range(0, edgedSpawnTiles.Count)];
            edgedSpawnTiles.Remove(spawnTile);

            Enemy_ScrObj spawningEnemy = spawnEnemies[i];
            Enemy enemy = Spawn(spawningEnemy, spawnTile);

            yield return null;
            while (enemy.animator.CurrentState_Playing()) yield return null;
        }
        yield break;
    }

    private IEnumerator Spawn_CurrentSpawnData()
    {
        if (_spawnedEnemies.Count > 0) yield break;

        StageData currentStageData = GameManager.instance.stageManager.stageMap.data.Current_StageData();
        if (currentStageData == null) yield break;

        Enemy_SpawnData spawnData = currentStageData.Current_EnemySpawnData();
        if (spawnData == null) yield break;

        yield return Sequential_DelaySpawn(spawnData);
    }
    private IEnumerator Update_SpawnData()
    {
        if (_spawnedEnemies.Count > 0) yield break;

        StageMap_Data stageMapData = GameManager.instance.stageManager.stageMap.data;

        StageData currentStageData = stageMapData.Current_StageData();
        if (currentStageData == null) yield break;

        if (currentStageData.Update_EnemySpawnData()) yield break;
        stageMapData.Complete_CurrentStageData();
    }


    // Hover
    private List<Tile> HoverIndicate_InteractRangeTiles(Enemy hoverEnemy)
    {
        if (hoverEnemy == null) return null;

        Tile currentTile = hoverEnemy.movement.currentTile;
        int interactRange = hoverEnemy.data.currentData.interactRange;

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(currentTile, interactRange);
        interactRangeTiles.Remove(currentTile);

        return interactRangeTiles;
    }
    private void HoverIndicate_TargetTile(Enemy hoverEnemy)
    {
        Tile indicateTargetTile = hoverEnemy.TargetTile();
        if (indicateTargetTile == null) return;

        indicateTargetTile.indicatorAnimController.Play_State(UIAnimation.Restricted);
    }

    private void Hover_Enemy()
    {
        GameManager manager = GameManager.instance;
        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        Enemy hoveringEnemy = Spawned_Enemy(hoveringTile);
        bool enemyStanding = hoveringEnemy != null;

        _enemyHoverToolTip.Toggle(enemyStanding);

        if (hoveringTile == null || hoveringTile.currentOccupant == null)
        {
            tileManager.Reset_TileIndicators();
            return;
        }
        if (enemyStanding == false) return;

        List<Tile> indicateTiles = HoverIndicate_InteractRangeTiles(hoveringEnemy);
        foreach (Tile tile in indicateTiles)
        {
            tile.indicatorAnimController.Play_State(UIAnimation.Toggle);
        }
        HoverIndicate_TargetTile(hoveringEnemy);

        // ToolTip
        CharacterScrObj enemyScrObj = hoveringEnemy.data.enemyScrObj;

        _enemyHoverToolTip.ToggleOn_CursorPoint(true);
        _enemyHoverToolTip.Update_Contents(enemyScrObj.toolTipBaseSprite, null, enemyScrObj.characterName, enemyScrObj.characterDescription);
    }


    // Actions
    private IEnumerator Run_EnemyActions()
    {
        Hero currentHero = GameManager.instance.heroManager.currentHero;
        if (currentHero != null && currentHero.data.currentData.currentHealth <= 0) yield break;

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