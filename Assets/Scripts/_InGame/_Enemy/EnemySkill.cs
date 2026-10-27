using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySkillTrigger
{
    PreMovement,
    AfterMovement,
    PreDamaging,
    AfterDamaging,
    Death
}

public enum EnemySkillTarget
{
    CurrentTile,
    DamagingTargetTile,
    InteractRangeTile,
    InteractRangeTiles
}

[System.Serializable]
public class EnemySkill_TriggerData
{
    [SerializeField] private EnemySkillTrigger _trigger;
    public EnemySkillTrigger trigger => _trigger;

    [SerializeField] private EnemySkillTarget _target;
    public EnemySkillTarget target => _target;

    [Space(10)]
    [SerializeField] private EnemySkill[] _enemySkills;
    public EnemySkill[] enemySkills => _enemySkills;
}

public abstract class EnemySkill : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _triggerCoolTime;
    [SerializeField][Range(0, 10)] private int _triggerRepeatCount;

    [SerializeField] private ActivateCondition_ScrObj[] _triggerConditions;
    public ActivateCondition_ScrObj[] triggerConditions => _triggerConditions;


    private Enemy _enemy;
    public Enemy enemy => _enemy;


    private int _currentTriggerCoolTime;
    public int currentTriggerCoolTime => _currentTriggerCoolTime;

    private EnemySkillTrigger _currentTrigger;
    public EnemySkillTrigger currentTrigger => _currentTrigger;

    private EnemySkillTarget _currentTarget;
    public EnemySkillTarget currentTarget => _currentTarget;


    // EnemySkillTarget
    public List<Tile> InteractRange_Tiles()
    {
        Tile currentTile = _enemy.movement.currentTile;
        int interactRange = _enemy.data.currentData.interactRange;

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(currentTile, interactRange);
        return interactRangeTiles;
    }

    public List<Tile> CurrentTarget_Tiles()
    {
        List<Tile> targetTiles = new();
        Tile currentTile = _enemy.movement.currentTile;


        switch (_currentTarget)
        {
            case EnemySkillTarget.CurrentTile:

                targetTiles.Add(currentTile);
                break;


            case EnemySkillTarget.DamagingTargetTile:

                Tile damagingTile = _enemy.damagingTile;
                if (damagingTile == null) break;

                targetTiles.Add(damagingTile);
                break;


            case EnemySkillTarget.InteractRangeTile:

                List<Tile> interactRangeTiles = InteractRange_Tiles();

                int rangeTileCount = interactRangeTiles.Count;
                if (rangeTileCount <= 0) break;

                targetTiles.Add(interactRangeTiles[Random.Range(0, rangeTileCount)]);
                break;


            case EnemySkillTarget.InteractRangeTiles: return InteractRange_Tiles();
        }
        return targetTiles;
    }
    public List<Tile> SpawnTarget_Tiles(int spawnCount)
    {
        List<Tile> targetTiles = new();

        Tile enemyCurrentTile = enemy.movement.currentTile;
        int interactRange = enemy.data.currentData.interactRange;

        TileManager tileManager = GameManager.instance.tileManager;

        List<Tile> interactRangeTiles = tileManager.Distanced_Tiles(enemyCurrentTile, interactRange);
        if (currentTrigger != EnemySkillTrigger.Death) interactRangeTiles.Remove(enemyCurrentTile);

        for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
        {
            if (interactRangeTiles[i].currentOccupant == null) continue;
            interactRangeTiles.RemoveAt(i);
        }


        switch (currentTarget)
        {
            case EnemySkillTarget.CurrentTile:

                targetTiles.Add(enemyCurrentTile);
                return targetTiles;


            case EnemySkillTarget.DamagingTargetTile:

                Tile damagingTile = enemy.damagingTile;
                if (damagingTile == null) return targetTiles;

                targetTiles.Add(damagingTile);
                return targetTiles;


            case EnemySkillTarget.InteractRangeTile:

                if (interactRangeTiles.Count <= 0) return targetTiles;

                targetTiles.Add(interactRangeTiles[UnityEngine.Random.Range(0, interactRangeTiles.Count)]);
                return targetTiles;


            case EnemySkillTarget.InteractRangeTiles:

                int maxSpawnCount = Mathf.Min(spawnCount, interactRangeTiles.Count);

                for (int i = 0; i < maxSpawnCount; i++)
                {
                    int randIndex = UnityEngine.Random.Range(0, interactRangeTiles.Count);

                    targetTiles.Add(interactRangeTiles[randIndex]);
                    interactRangeTiles.RemoveAt(randIndex);
                }
                return targetTiles;
        }
        return targetTiles;
    }


    // Main
    public void Set_Data(Enemy enemy, EnemySkillTrigger setTrigger, EnemySkillTarget setTarget)
    {
        _enemy = enemy;
        _currentTrigger = setTrigger;
        _currentTarget = setTarget;
    }

    public IEnumerator Run_SkillTrigger()
    {
        if (_currentTriggerCoolTime > 0)
        {
            _currentTriggerCoolTime--;
            yield break;
        }
        _currentTriggerCoolTime = _triggerCoolTime;

        int repeatCount = Mathf.Max(1, _triggerRepeatCount);

        for (int i = 0; i < repeatCount; i++)
        {
            if (CustomData_Utility.AllConditions_ActivateAvailable(_enemy.movement.currentTile, _triggerConditions) == false) yield break;
            yield return Trigger_Skill();
        }
    }
    public abstract IEnumerator Trigger_Skill();
}