using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_CardRemover : CardSkill
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _targetCards;
    public Card_ScrObj[] targetCards => _targetCards;


    // data
    private List<Card> TargetTile_Cards()
    {
        CardManager cardManager = GameManager.instance.cardManager;

        List<Tile> targetTiles = CurrentTarget_Tiles();
        List<Card> targetTileCards = new();

        for (int i = 0; i < targetTiles.Count; i++)
        {
            Tile tile = targetTiles[i];

            Card placedCard = cardManager.PlacedCard(tile);
            if (placedCard == null) continue;

            Card_ScrObj cardScrObj = placedCard.data.cardScrObj;
            for (int j = 0; j < targetCards.Length; j++)
            {
                if (targetCards[j] != cardScrObj) continue;

                targetTileCards.Add(placedCard);
                break;
            }
        }
        return targetTileCards;
    }


    // abstract
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