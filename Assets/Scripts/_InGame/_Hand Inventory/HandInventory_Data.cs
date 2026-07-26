using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInventory_Data
{
    private List<CardData> _deckCardDatas = new();
    public List<CardData> deckCardDatas => _deckCardDatas;

    private List<CardData> _handCardDatas = new();
    public List<CardData> handCardDatas => _handCardDatas;


    // New
    public HandInventory_Data(List<CardData> startingDeckCardDatas)
    {
        _deckCardDatas = new(startingDeckCardDatas);
    }
}