using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckBuild_Data
{
    [ES3Serializable] private List<List<Card_ScrObj>> _buildGroupCards = new();
    public List<List<Card_ScrObj>> buildGroupCards => _buildGroupCards;

    [ES3Serializable] private List<CardData> _builtDeckCardDatas = new();
    public List<CardData> builtDeckCardDatas => _builtDeckCardDatas;

    [ES3Serializable] private List<Card_ScrObj> _cardPool = new();
    public List<Card_ScrObj> cardPool => _cardPool;


    // Data
    public List<Card_ScrObj> TargetGroup_CurrentCards(int groupIndex)
    {
        groupIndex = Mathf.Clamp(groupIndex, 0, _buildGroupCards.Count - 1);
        return _buildGroupCards[groupIndex];
    }
}