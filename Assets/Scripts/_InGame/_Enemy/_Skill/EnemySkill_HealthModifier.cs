using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill_HealthModifier : EnemySkill
{
    [Space(20)]
    [SerializeField] private bool _includeEnemies;
    [SerializeField] private bool _duplicateTargeting;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _targetingCount;
    [SerializeField][Range(-100, 100)] private int _healthModifyValue;


    private List<IInteractable> InteractRange_TargetInteractables()
    {
        List<Tile> interactRangeTiles = InteractRange_Tiles();
        List<IInteractable> interactables = new();

        for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = interactRangeTiles[i].CurrentOccupant_Interactable();
            if (interactable != null && (interactable is not Enemy || _includeEnemies)) continue;

            interactRangeTiles.RemoveAt(i);
        }
        if (_duplicateTargeting == false) interactRangeTiles.Remove(enemy.damagingTile);

        int rangeTileCount = interactRangeTiles.Count;
        if (rangeTileCount <= 0) return interactables;

        foreach (Tile tile in interactRangeTiles)
        {
            interactables.Add(tile.CurrentOccupant_Interactable());
        }
        return interactables;
    }
    private List<IInteractable> Target_Interactables()
    {
        List<IInteractable> targets = new();
        if (_targetingCount <= 0) return targets;

        List<IInteractable> rangedInteractables = InteractRange_TargetInteractables();

        switch (currentTarget)
        {
            case EnemySkillTarget.CurrentTile:

                if (_includeEnemies == false) return targets;

                targets.Add(enemy);
                return targets;


            case EnemySkillTarget.DamagingTargetTile:

                Tile damagingTile = enemy.damagingTile;
                if (damagingTile == null) return targets;

                IInteractable damagaingInteractable = damagingTile.CurrentOccupant_Interactable();

                if (damagaingInteractable == null) return targets;
                if (damagaingInteractable is Enemy && _includeEnemies == false) return targets;

                targets.Add(damagaingInteractable);
                return targets;


            case EnemySkillTarget.InteractRangeTile:

                if (rangedInteractables.Count <= 0) return targets;

                targets.Add(rangedInteractables[Random.Range(0, rangedInteractables.Count)]);
                return targets;


            case EnemySkillTarget.InteractRangeTiles:

                for (int i = 0; i < _targetingCount; i++)
                {
                    if (rangedInteractables.Count <= 0) break;

                    int randIndex = Random.Range(0, rangedInteractables.Count);
                    targets.Add(rangedInteractables[randIndex]);

                    if (_duplicateTargeting) continue;
                    rangedInteractables.RemoveAt(randIndex);
                }
                return targets;
        }
        return targets;
    }

    public override IEnumerator Trigger_Skill()
    {
        List<IInteractable> targetInteractables = Target_Interactables();

        for (int i = 0; i < targetInteractables.Count; i++)
        {
            InteractionData data = targetInteractables[i].interactionData;

            enemy.interactionData.Modify_OtherData_CurrentHealth(data, _healthModifyValue);
            while (data.dataUpdating) yield return null;
        }
        yield break;
    }
}
