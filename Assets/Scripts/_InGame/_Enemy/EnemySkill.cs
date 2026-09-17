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
    private Enemy _enemy;
    public Enemy enemy => _enemy;

    private EnemySkillTrigger _currentTrigger;
    public EnemySkillTrigger currentTrigger => _currentTrigger;

    private EnemySkillTarget _currentTarget;
    public EnemySkillTarget currentTarget => _currentTarget;


    // EnemySkillTarget
    /*
    public List<Tile> CurrentTarget_Tiles()
    {
        List<Tile> targetTiles = new();

        Tile placedTile = _card.placedTile;
        int interactRange = card.data.currentData.interactRange;

        List<Tile> targetingTiles = new(card.tileTargeting.recentTargetingTiles);

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(placedTile, interactRange);
        interactRangeTiles.Remove(placedTile);

        switch (_currentTarget)
        {
            case CardSkillTarget.CurrentTile:
                targetTiles.Add(placedTile);
                break;

            case CardSkillTarget.ActionTargetingTile:
                Tile targetingTile = _card.targetingTile;
                if (targetingTile == null) break;

                targetTiles.Add(targetingTile);
                break;

            case CardSkillTarget.TargetingTiles:
                return targetingTiles;

            case CardSkillTarget.InteractRangeTile:
                int rangeTileCount = interactRangeTiles.Count;
                if (rangeTileCount <= 0) break;

                targetTiles.Add(interactRangeTiles[Random.Range(0, rangeTileCount)]);
                break;

            case CardSkillTarget.InteractRangeTiles:
                return interactRangeTiles;
        }
        return targetTiles;
    }
    */


    // Main
    public void Set_Data(Enemy enemy, EnemySkillTrigger setTrigger, EnemySkillTarget setTarget)
    {
        _enemy = enemy;
        _currentTrigger = setTrigger;
        _currentTarget = setTarget;
    }

    public abstract IEnumerator Trigger_Skill();
}