using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSkillTrigger
{
    Place,
    PreUpdate,
    AfterUpdate,
    PreTargeting,
    AfterTargeting,
    HealthUpdate,
    Death
}

public enum CardSkillTarget
{
    CurrentTile,
    TargetingTile,
    TargetingTiles,
    InteractRangeTile,
    InteractRangeTiles
}

[System.Serializable]
public class CardSkill_TriggerData
{
    [SerializeField] private CardSkillTrigger _trigger;
    public CardSkillTrigger trigger => _trigger;

    [SerializeField] private CardSkillTarget _target;
    public CardSkillTarget target => _target;

    [Space(10)]
    [SerializeField] private CardSkill[] _cardSkills;
    public CardSkill[] cardSkills => _cardSkills;
}

public abstract class CardSkill : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private ActivateCondition_ScrObj[] _triggerConditions;
    public ActivateCondition_ScrObj[] triggerConditions => _triggerConditions;

    [Space(10)]
    [SerializeField] private bool _duplicateTargeting;
    [SerializeField] private bool _includeCurrentTile;


    private Card _card;
    public Card card => _card;

    private CardSkillTrigger _currentTrigger;
    public CardSkillTrigger currentTrigger => _currentTrigger;

    private CardSkillTarget _currentTarget;
    public CardSkillTarget currentTarget => _currentTarget;


    // Card Skill Target
    public List<Tile> InteractRange_Tiles()
    {
        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(_card.placedTile, _card.data.currentData.interactRange);
        if (_includeCurrentTile) return interactRangeTiles;

        interactRangeTiles.Remove(_card.placedTile);
        return interactRangeTiles;
    }

    public List<Tile> CurrentTarget_Tiles()
    {
        List<Tile> targetTiles = new();

        Tile placedTile = _card.placedTile;
        List<Tile> interactRangeTiles = InteractRange_Tiles();


        switch (_currentTarget)
        {
            case CardSkillTarget.CurrentTile:

                if (_includeCurrentTile == false) return targetTiles;
                targetTiles.Add(placedTile);

                return targetTiles;


            case CardSkillTarget.TargetingTile:

                Tile targetingTile = _card.runTimeActionData.targetingTile;
                if (targetingTile == null) return targetTiles;

                targetTiles.Add(targetingTile);
                return targetTiles;


            case CardSkillTarget.TargetingTiles: return new(_card.tileTargeting.recentTargetingTiles);


            case CardSkillTarget.InteractRangeTile:

                int rangeTileCount = interactRangeTiles.Count;
                if (rangeTileCount <= 0) return targetTiles;

                int randIndex = Random.Range(0, rangeTileCount);
                targetTiles.Add(interactRangeTiles[randIndex]);

                return targetTiles;


            case CardSkillTarget.InteractRangeTiles: return interactRangeTiles;
        }
        return targetTiles;
    }
    public List<Tile> SpawnTarget_Tiles(int spawnCount)
    {
        if (spawnCount <= 0) return new();

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
            case CardSkillTarget.CurrentTile:

                if (currentTrigger == CardSkillTrigger.Death) return currentTargetTiles;

                currentTargetTiles.Remove(_card.placedTile);
                return currentTargetTiles;


            case CardSkillTarget.TargetingTile: return currentTargetTiles;


            case CardSkillTarget.TargetingTiles:

                for (int i = currentTargetTiles.Count - 1; i >= 0; i--)
                {
                    if (currentTargetTiles.Count <= spawnCount) break;
                    currentTargetTiles.RemoveAt(i);
                }
                return currentTargetTiles;


            case CardSkillTarget.InteractRangeTile:

                if (interactRangeTiles.Count <= 0) return interactRangeTiles;

                List<Tile> interactRangeTile = new()
                {
                    interactRangeTiles[UnityEngine.Random.Range(0, interactRangeTiles.Count)]
                };
                return interactRangeTile;


            case CardSkillTarget.InteractRangeTiles:

                int removeCount = interactRangeTiles.Count - Mathf.Min(spawnCount, interactRangeTiles.Count);

                for (int i = 0; i < removeCount; i++)
                {
                    int randIndex = Random.Range(0, interactRangeTiles.Count);
                    interactRangeTiles.RemoveAt(randIndex);
                }
                return interactRangeTiles;
        }
        return new();
    }

    public List<IInteractable> CurrentTarget_Interactables()
    {
        List<IInteractable> targetInteractables = new();

        List<Tile> interactRangeTiles = InteractRange_Tiles();
        for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
        {
            if (interactRangeTiles[i].CurrentOccupant_Interactable() != null) continue;
            interactRangeTiles.RemoveAt(i);
        }

        List<Tile> currentTargetTiles = CurrentTarget_Tiles();
        for (int i = currentTargetTiles.Count - 1; i >= 0; i--)
        {
            if (currentTargetTiles[i].CurrentOccupant_Interactable() != null) continue;
            currentTargetTiles.RemoveAt(i);
        }


        switch (currentTarget)
        {
            case CardSkillTarget.TargetingTiles:

                foreach (Tile tile in interactRangeTiles)
                {
                    targetInteractables.Add(tile.CurrentOccupant_Interactable());
                }
                return targetInteractables;


            case CardSkillTarget.InteractRangeTile:

                int rangeTileCount = interactRangeTiles.Count;
                if (rangeTileCount <= 0) return targetInteractables;

                targetInteractables.Add(interactRangeTiles[Random.Range(0, rangeTileCount)].CurrentOccupant_Interactable());
                return targetInteractables;
        }


        foreach (Tile tile in currentTargetTiles)
        {
            targetInteractables.Add(tile.CurrentOccupant_Interactable());
        }
        return targetInteractables;
    }
    public List<IInteractable> CurrentTarget_Interactables(int targetingCount)
    {
        List<IInteractable> targetInteractables = new();
        if (targetingCount <= 0) return targetInteractables;

        List<Tile> interactRangeTiles = InteractRange_Tiles();
        for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
        {
            if (interactRangeTiles[i].CurrentOccupant_Interactable() != null) continue;
            interactRangeTiles.RemoveAt(i);
        }

        List<Tile> currentTargetTiles = CurrentTarget_Tiles();
        for (int i = currentTargetTiles.Count - 1; i >= 0; i--)
        {
            if (currentTargetTiles[i].CurrentOccupant_Interactable() != null) continue;
            currentTargetTiles.RemoveAt(i);
        }


        switch (currentTarget)
        {
            case CardSkillTarget.TargetingTiles:

                for (int i = 0; i < currentTargetTiles.Count; i++)
                {
                    if (targetInteractables.Count >= targetingCount) break;
                    targetInteractables.Add(currentTargetTiles[i].CurrentOccupant_Interactable());
                }
                return targetInteractables;


            case CardSkillTarget.InteractRangeTile:

                if (interactRangeTiles.Count <= 0) return targetInteractables;

                targetInteractables.Add(interactRangeTiles[Random.Range(0, interactRangeTiles.Count)].CurrentOccupant_Interactable());
                return targetInteractables;


            case CardSkillTarget.InteractRangeTiles:

                for (int i = 0; i < targetingCount; i++)
                {
                    if (interactRangeTiles.Count <= 0) return targetInteractables;

                    int randIndex = Random.Range(0, interactRangeTiles.Count);
                    targetInteractables.Add(interactRangeTiles[randIndex].CurrentOccupant_Interactable());

                    if (_duplicateTargeting) continue;
                    interactRangeTiles.RemoveAt(randIndex);
                }
                return targetInteractables;
        }


        foreach (Tile tile in currentTargetTiles)
        {
            targetInteractables.Add(tile.CurrentOccupant_Interactable());
        }
        return targetInteractables;
    }


    // Main
    public void Set_Data(Card card, CardSkillTrigger setTrigger, CardSkillTarget setTarget)
    {
        _card = card;

        _currentTrigger = setTrigger;
        _currentTarget = setTarget;
    }

    public IEnumerator Run_SkillTrigger()
    {
        if (CustomData_Utility.AllConditions_ActivateAvailable(_card.placedTile, _triggerConditions) == false) yield break;

        yield return Trigger_Skill();
    }
    public abstract IEnumerator Trigger_Skill();
}
