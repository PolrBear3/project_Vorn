using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager_Data
{
    private Dictionary<CardData, Vector2> _placedCardDatas = new();
    public Dictionary<CardData, Vector2> placedCardDatas => _placedCardDatas;


    public bool Add_PlacedData(Card placedCard)
    {
        Vector2 placedCardPos = placedCard.placedTile.data.position;

        foreach (var cardData in _placedCardDatas)
        {
            if (cardData.Value != placedCardPos) continue;
            return false;
        }

        _placedCardDatas.Add(placedCard.data, placedCardPos);
        return true;
    }

    public CardData PositionPlaced_CardData(Vector2 placedTilePosition)
    {
        foreach (var cardData in _placedCardDatas)
        {
            if (placedTilePosition != cardData.Value) continue;
            return cardData.Key;
        }
        return null;
    }
}
