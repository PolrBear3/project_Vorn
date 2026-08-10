using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager_Data
{
    private Dictionary<CardData, Vector2> _placedCardDatas = new();
    public Dictionary<CardData, Vector2> placedCardDatas => _placedCardDatas;


    // Data
    public void Save_PlacedCardDatas(List<Card> placedCards)
    {
        for (int i = 0; i < placedCards.Count; i++)
        {
            Card card = placedCards[i];
            _placedCardDatas.Add(card.data, card.placedTile.data.position);
        }
    }
}
