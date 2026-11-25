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
    [Space(20)]
    [SerializeReference]
    [Tooltip("Condition for triggering the skill.")]
    [SerializeField] private ActivateCondition_ScrObj[] _currentTileConditions;
    [SerializeReference]
    [Tooltip("Condition for filtering skill trigger tiles.")]
    [SerializeField] private ActivateCondition_ScrObj[] _candidateTargetTileConditions;

    [Space(20)]
    [SerializeField] private bool _includeCurrentTile;
    [SerializeField] private bool _duplicateTargeting;


    private Card _card;
    public Card card => _card;

    private CardSkillTrigger _currentTrigger;
    public CardSkillTrigger currentTrigger => _currentTrigger;

    private CardSkillTarget _currentTarget;
    public CardSkillTarget currentTarget => _currentTarget;


    // Targeting Datas
    public List<Tile> InteractRange_Tiles()
    {
        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(_card.placedTile, _card.data.currentData.interactRange);
        interactRangeTiles = CustomData_Utility.Conditions_ActivateAvailableTiles(interactRangeTiles, _candidateTargetTileConditions);

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

                if (_includeCurrentTile == false) break;
                if (CustomData_Utility.Conditions_ActivateAvailable(placedTile, _candidateTargetTileConditions) == false) break;

                targetTiles.Add(placedTile);
                break;


            case CardSkillTarget.TargetingTile:

                Tile actionTargetingTile = _card.actionTargetingTile;

                if (actionTargetingTile == null) break;
                if (CustomData_Utility.Conditions_ActivateAvailable(actionTargetingTile, _candidateTargetTileConditions) == false) break;

                targetTiles.Add(actionTargetingTile);
                break;


            case CardSkillTarget.TargetingTiles:

                targetTiles = CustomData_Utility.Conditions_ActivateAvailableTiles(new(_card.tileTargeting.recentTargetingTiles), _candidateTargetTileConditions);
                break;


            case CardSkillTarget.InteractRangeTile:

                int rangeTileCount = interactRangeTiles.Count;
                if (rangeTileCount <= 0) break;

                int randIndex = Random.Range(0, rangeTileCount);

                targetTiles.Add(interactRangeTiles[randIndex]);
                break;


            case CardSkillTarget.InteractRangeTiles:

                targetTiles = interactRangeTiles;
                break;
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


    private List<Tile> OccupantTiles(List<Tile> filteringTiles)
    {
        List<Tile> nonOccupantTiles = new();

        for (int i = 0; i < filteringTiles.Count; i++)
        {
            Tile filteringTile = filteringTiles[i];

            if (filteringTile.CurrentOccupant_Interactable() == null) continue;
            nonOccupantTiles.Add(filteringTile);
        }
        return nonOccupantTiles;
    }

    public List<IInteractable> CurrentTarget_Interactables()
    {
        List<IInteractable> targetInteractables = new();


        List<Tile> interactRangeTiles = OccupantTiles(InteractRange_Tiles());
        List<Tile> currentTargetTiles = OccupantTiles(CurrentTarget_Tiles());


        switch (currentTarget)
        {
            case CardSkillTarget.TargetingTiles:

                foreach (Tile tile in currentTargetTiles)
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


        List<Tile> interactRangeTiles = OccupantTiles(InteractRange_Tiles());
        List<Tile> currentTargetTiles = OccupantTiles(CurrentTarget_Tiles());


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
        if (CustomData_Utility.Conditions_ActivateAvailable(_card.placedTile, _currentTileConditions) == false) yield break;
        yield return Trigger_Skill();
    }
    public abstract IEnumerator Trigger_Skill();
}
