using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
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
        EventBus_GlobalController.UnRegister(Subscribe_All);
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
        endTurnBus.Register(0, UnToggle_EnemyStats);
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
        endTurnBus.UnRegister(UnToggle_EnemyStats);
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
    public List<Enemy> Spawned_Enemies()
    {
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

        spawnTile.Update_CurrentOccupant(enemyObj);

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
    public IEnumerator DelaySpawn(Enemy_ScrObj spawnEnemy, Tile spawnTile)
    {
        Enemy enemy = Spawn(spawnEnemy, spawnTile);

        yield return null;
        while (enemy.animator.CurrentState_Playing()) yield return null;
    }

    private IEnumerator Sequential_DelaySpawn(Enemy_SpawnData spawnData)
    {
        List<Enemy_ScrObj> spawnEnemies = spawnData.Spawn_Enemies();
        List<Tile> edgedSpawnTiles = GameManager.instance.tileManager.Edged_Tiles();

        for (int i = 0; i < spawnEnemies.Count; i++)
        {
            Tile spawnTile = edgedSpawnTiles[UnityEngine.Random.Range(0, edgedSpawnTiles.Count)];
            edgedSpawnTiles.Remove(spawnTile);

            yield return DelaySpawn(spawnEnemies[i], spawnTile);
        }
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
    private IEnumerator Spawn_CurrentSpawnData()
    {
        if (_spawnedEnemies.Count > 0) yield break;

        StageData currentStageData = GameManager.instance.stageManager.stageMap.data.Current_StageData();
        if (currentStageData == null) yield break;

        Enemy_SpawnData spawnData = currentStageData.Current_EnemySpawnData();
        if (spawnData == null) yield break;

        yield return Sequential_DelaySpawn(spawnData);
    }


    // Hover
    private List<Tile> Enemy_InteractRangeTiles(Enemy targetEnemy)
    {
        if (targetEnemy == null) return null;

        Tile currentTile = targetEnemy.movement.currentTile;
        int interactRange = targetEnemy.data.currentData.interactRange;

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(currentTile, interactRange);
        interactRangeTiles.Remove(currentTile);

        return interactRangeTiles;
    }

    private void Indicate_EnemyCurrentTile(Enemy hoverEnemy)
    {
        Tile indicateTargetTile = hoverEnemy.TargetTile();
        if (indicateTargetTile == null) return;

        indicateTargetTile.indicatorAnimController.Play_State(UIAnimation.Restricted);
    }
    private void Indicate_EnemyTiles(Enemy targetEnemy)
    {
        if (targetEnemy == null) return;
        List<Tile> indicateTiles = Enemy_InteractRangeTiles(targetEnemy);

        foreach (Tile tile in indicateTiles)
        {
            tile.indicatorAnimController.Play_State(UIAnimation.Toggle);
        }
        Indicate_EnemyCurrentTile(targetEnemy);
    }

    private void ToggleEnemy_StatIndications(Enemy toggleEnemy)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            bool isHoveringEnemy = enemy == toggleEnemy;

            enemy.healthBar.Toggle_ValueBox(isHoveringEnemy);

            HealthModify_Indicator modifyIndicator = enemy.modifyIndicator;
            modifyIndicator.gameObject.SetActive(isHoveringEnemy);

            if (isHoveringEnemy == false) continue;
            modifyIndicator.Update_ModifyValue(enemy.data.currentData.healthModifyValue);
        }
    }
    private void UnToggle_EnemyStats()
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            enemy.healthBar.Toggle_ValueBox(false);
            enemy.modifyIndicator.gameObject.SetActive(false);
        }
    }

    private void Hover_Enemy()
    {
        GameManager manager = GameManager.instance;
        StageManager stageManager = manager.stageManager;

        if (stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (stageManager.endStageEventBus.DelayBus_Running()) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        Enemy hoveringEnemy = Spawned_Enemy(hoveringTile);
        ToggleEnemy_StatIndications(hoveringEnemy);

        if (hoveringTile == null || hoveringTile.currentOccupant == null)
        {
            tileManager.Reset_TileIndicators();
            _enemyHoverToolTip.Toggle(false);

            return;
        }
        if (hoveringEnemy == null) return;

        Indicate_EnemyTiles(hoveringEnemy);

        // ToolTip
        CharacterScrObj enemyScrObj = hoveringEnemy.data.enemyScrObj;

        _enemyHoverToolTip.ToggleOn_CursorPoint(true);
        _enemyHoverToolTip.Update_Contents(enemyScrObj.toolTipBaseSprite, null, enemyScrObj.characterName, enemyScrObj.characterDescription);
    }


    // Actions
    private IEnumerator Run_EnemyActions()
    {
        Hero currentHero = GameManager.instance.heroManager.spawnedHero;
        if (currentHero != null && currentHero.data.currentData.currentHealth <= 0) yield break;

        List<Enemy> actionEnemies = new(_spawnedEnemies);
        for (int i = actionEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = actionEnemies[i];
            if (enemy == null) continue;

            StartCoroutine(enemy.Run_EndTurnActions());
            while (enemy != null && (enemy.runTimeActionData.actionRunning || enemy.interactionDataUpdater.healthHandleUpdateRunning)) yield return null;
        }

        yield break;
    }
}
