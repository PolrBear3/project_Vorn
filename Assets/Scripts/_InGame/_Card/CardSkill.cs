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
    ActionTargetingTile,
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
    private Card _card;
    public Card card => _card;

    private CardSkillTrigger _currentTrigger;
    public CardSkillTrigger currentTrigger => _currentTrigger;

    private CardSkillTarget _currentTarget;
    public CardSkillTarget currentTarget => _currentTarget;


    // CardSkillTarget
    public List<Tile> CurrentTarget_Tiles()
    {
        List<Tile> targetTiles = new();
        Tile placedTile = _card.placedTile;

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
                return new(_card.tileTargeting.recentTargetingTiles);

            case CardSkillTarget.InteractRangeTile:
                {
                    List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(placedTile, _card.data.currentData.interactRange);
                    interactRangeTiles.Remove(placedTile);

                    int rangeTileCount = interactRangeTiles.Count;
                    if (rangeTileCount <= 0) break;

                    targetTiles.Add(interactRangeTiles[Random.Range(0, rangeTileCount)]);
                    break;
                }

            case CardSkillTarget.InteractRangeTiles:
                {
                    List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(placedTile, _card.data.currentData.interactRange);
                    interactRangeTiles.Remove(placedTile);

                    return interactRangeTiles;
                }
        }
        return targetTiles;
    }


    // Main
    public void Set_Data(Card card, CardSkillTrigger setTrigger, CardSkillTarget setTarget)
    {
        _card = card;

        _currentTrigger = setTrigger;
        _currentTarget = setTarget;
    }

    public abstract IEnumerator Trigger_Skill();
}