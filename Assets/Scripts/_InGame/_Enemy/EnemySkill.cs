using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySkillTrigger
{
    PreMovement,
    AfterMovement,
    PreDamaging,
    AfterDamaging
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
    [SerializeField] private ActivateCondition_ScrObj[] _triggerConditions;
    public ActivateCondition_ScrObj[] triggerConditions => _triggerConditions;


    private Enemy _enemy;
    public Enemy enemy => _enemy;

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
        interactRangeTiles.Remove(currentTile);

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


    // Main
    public void Set_Data(Enemy enemy, EnemySkillTrigger setTrigger, EnemySkillTarget setTarget)
    {
        _enemy = enemy;
        _currentTrigger = setTrigger;
        _currentTarget = setTarget;
    }

    public IEnumerator Run_SkillTrigger()
    {
        if (CustomData_Utility.AllConditions_ActivateAvailable(_enemy.movement.currentTile, _triggerConditions) == false) yield break;

        yield return Trigger_Skill();
    }
    public abstract IEnumerator Trigger_Skill();
}