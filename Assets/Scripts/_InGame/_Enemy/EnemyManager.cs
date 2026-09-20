using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyManager_Data _data = new();  // for save & load
    public EnemyManager_Data data => _data;

    private List<Enemy> _spawnedEnemies = new();
    public List<Enemy> spawnedEnemies => _spawnedEnemies;

    private Coroutine _spawnCoroutine;


    [Space(20)]
    [SerializeField] private ToolTip _enemyHoverToolTip;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller endTurnBus = stageManager.endTurnEventBus;

        stageManager.stageSetEventBus.UnRegister(Run_DelaySpawn);
        endTurnBus.UnRegister(Run_EnemyActions);

        manager.tileManager.tileHoverEventBus.UnRegister(Hover_Enemy);
        endTurnBus.OnSequentialDelayFinish -= Hover_Enemy;

        endTurnBus.UnRegister(_enemyHoverToolTip.UnToggle);
    }


    // Data
    private void Set_Data()
    {
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller endTurnBus = stageManager.endTurnEventBus;

        stageManager.stageSetEventBus.Register(0, Run_DelaySpawn);
        endTurnBus.Register(3, Run_EnemyActions);

        manager.tileManager.tileHoverEventBus.Register(0, Hover_Enemy);
        endTurnBus.OnSequentialDelayFinish += Hover_Enemy;

        endTurnBus.Register(0, _enemyHoverToolTip.UnToggle);
    }

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
    private void Hover_Enemy(bool tileTargetingToggled)
    {

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