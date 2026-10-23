using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckBuild_Data
{
    [ES3Serializable] private List<Card_ScrObj> _cardPool = new();
    public List<Card_ScrObj> cardPool => _cardPool;

    [ES3Serializable] private List<List<Card_ScrObj>> _buildGroupCards = new();
    public List<List<Card_ScrObj>> buildGroupCards => _buildGroupCards;

    [ES3Serializable] private List<CardData> _builtDeckCardDatas = new();
    public List<CardData> builtDeckCardDatas => _builtDeckCardDatas;

    [ES3Serializable] private bool _deckBuildComplete;
    public bool deckBuildComplete => _deckBuildComplete;


    // Data
    public List<CardData> CardPool_CardDatas()
    {
        List<CardData> datas = new();

        foreach (Card_ScrObj card in cardPool)
        {
            datas.Add(new(card));
        }
        return datas;
    }
    public List<Card_ScrObj> TargetGroup_CurrentCards(int groupIndex)
    {
        groupIndex = Mathf.Clamp(groupIndex, 0, _buildGroupCards.Count - 1);
        return _buildGroupCards[groupIndex];
    }

    public bool Complete_DeckBuild(int buildCardCount)
    {
        if (_builtDeckCardDatas.Count < buildCardCount) return false;

        _deckBuildComplete = true;
        return true;
    }


    // Debug
    public void Debug_BuiltDeckCards()
    {
        Dictionary<Card_ScrObj, int> cardCountData = new();

        for (int i = 0; i < _builtDeckCardDatas.Count; i++)
        {
            Card_ScrObj card = _builtDeckCardDatas[i].cardScrObj;

            if (cardCountData.ContainsKey(card))
            {
                cardCountData[card]++;
                continue;
            }
            cardCountData[card] = 1;
        }
        foreach (var data in cardCountData)
        {
            Debug.Log(data.Key + ": " + data.Value);
        }
    }
}