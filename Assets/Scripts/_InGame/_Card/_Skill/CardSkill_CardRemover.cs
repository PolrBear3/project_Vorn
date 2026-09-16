using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_CardRemover : CardSkill
{
    [SerializeField] private Card_ScrObj[] _targetCards;

    private List<Card> TargetTile_Cards()
    {
        CardManager cardManager = GameManager.instance.cardManager;

        List<Tile> targetTiles = CurrentTarget_Tiles();
        List<Card> targetCards = new();

        for (int i = 0; i < targetTiles.Count; i++)
        {
            Tile tile = targetTiles[i];

            Card placedCard = cardManager.PlacedCard(tile);
            if (placedCard == null) continue;

            Card_ScrObj cardScrObj = placedCard.data.cardScrObj;
            for (int j = 0; j < _targetCards.Length; j++)
            {
                if (_targetCards[j] != cardScrObj) continue;

                targetCards.Add(placedCard);
                break;
            }
        }
        return targetCards;
    }

    // from abstract Trigger_Skill
    public override IEnumerator Trigger_Skill()
    {
        List<Card> removeCards = TargetTile_Cards();

        for (int i = removeCards.Count - 1; i >= 0; i--)
        {
            Card removeCard = removeCards[i];
            if (removeCard == null) continue;

            InteractionData data = removeCard.data.currentData;
            data.Update_CurrentHealth(0);

            yield return null;
            while (data.dataUpdating) yield return null;
        }
        yield break;
    }
}
