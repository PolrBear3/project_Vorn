using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySkillTrigger
{
    Spawn,
    PreMovement,
    AfterMovement,
    PreDamaging,
    AfterDamaging,
    HealthUpdate,
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

[System.Serializable]
public class EnemySkill_ConfigurationData : Skill_ConfigurationData
{
    [Space(10)]
    [SerializeField][Range(0, 10)] private int _activationCoolTime;
    public int activationCoolTime => _activationCoolTime;
}

public abstract class EnemySkill : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private EnemySkill_ConfigurationData _configurationData;

    [Space(20)]
    [Tooltip("Condition for triggering the skill.")]
    [SerializeField] private ActivateCondition_ScrObj[] _triggerConditions;

    [Tooltip("Condition for filtering skill trigger tiles.")]
    [SerializeField] private ActivateCondition_ScrObj[] _candidateTargetTileConditions;


    private Enemy _enemy;
    public Enemy enemy => _enemy;

    private EnemySkill_RunTimeData _runTimeData = new();
    public EnemySkill_RunTimeData runTimeData => _runTimeData;

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
        interactRangeTiles = CustomData_Utility.Conditions_ActivateAvailableTiles(interactRangeTiles, _candidateTargetTileConditions);

        return interactRangeTiles;
    }

    public List<Tile> CurrentTarget_Tiles()
    {
        List<Tile> targetTiles = new();
        Tile currentTile = _enemy.movement.currentTile;


        switch (_currentTarget)
        {
            case EnemySkillTarget.CurrentTile:

                if (CustomData_Utility.Conditions_ActivateAvailable(currentTile, _candidateTargetTileConditions) == false) break;

                targetTiles.Add(currentTile);
                break;


            case EnemySkillTarget.DamagingTargetTile:

                Tile damagingTile = _enemy.damagingTile;

                if (damagingTile == null) break;
                if (CustomData_Utility.Conditions_ActivateAvailable(damagingTile, _candidateTargetTileConditions) == false) break;

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
        List<Tile> currentTargetTiles = CurrentTarget_Tiles();
        for (int i = currentTargetTiles.Count - 1; i >= 0; i--)
        {
            if (currentTargetTiles[i].currentOccupant == null) continue;
            currentTargetTiles.RemoveAt(i);
        }

        List<Tile> interactRangeTiles = InteractRange_Tiles();
        for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
        {
            if (interactRangeTiles[i].currentOccupant == null) continue;
            interactRangeTiles.RemoveAt(i);
        }


        switch (currentTarget)
        {
            case EnemySkillTarget.CurrentTile:

                if (currentTrigger == EnemySkillTrigger.Death) return currentTargetTiles;

                currentTargetTiles.Remove(enemy.movement.currentTile);
                return currentTargetTiles;


            case EnemySkillTarget.DamagingTargetTile: return currentTargetTiles;


            case EnemySkillTarget.InteractRangeTile:

                if (interactRangeTiles.Count <= 0) return interactRangeTiles;

                List<Tile> interactRangeTile = new()
                {
                    interactRangeTiles[UnityEngine.Random.Range(0, interactRangeTiles.Count)]
                };
                return interactRangeTile;


            case EnemySkillTarget.InteractRangeTiles:

                int maxSpawnCount = Mathf.Min(spawnCount, interactRangeTiles.Count);

                for (int i = 0; i < maxSpawnCount; i++)
                {
                    int randIndex = UnityEngine.Random.Range(0, interactRangeTiles.Count);
                    interactRangeTiles.RemoveAt(randIndex);
                }
                return interactRangeTiles;
        }

        return new();
    }


    // Main
    public void Set_Data(Enemy enemy, EnemySkillTrigger setTrigger, EnemySkillTarget setTarget)
    {
        _enemy = enemy;

        _currentTrigger = setTrigger;
        _currentTarget = setTarget;

        _runTimeData.Update_ActivationCoolTime(_configurationData.activationCoolTime);
    }

    public IEnumerator Trigger_Skill()
    {
        if (_configurationData.On_ActivationDelay(_enemy.data.currentData.turnCount)) yield break;
        if (_configurationData.MaxCount_Activated(_runTimeData.activationCount)) yield break;

        if (CustomData_Utility.Conditions_ActivateAvailable(_enemy.movement.currentTile, _triggerConditions) == false) yield break;

        if (_runTimeData.On_ActivationCoolTime(_configurationData.activationCoolTime)) yield break;
        _runTimeData.Track_ActivationCount();

        yield return Activate_Skill();
    }
    public abstract IEnumerator Activate_Skill();
}
