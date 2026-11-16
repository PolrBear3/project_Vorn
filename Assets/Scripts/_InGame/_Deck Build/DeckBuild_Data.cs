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


    [ES3Serializable] private int _deckUpdateCompleteCount;
    public int deckUpdateCompleteCount => _deckUpdateCompleteCount;

    [ES3Serializable] private List<Card_ScrObj> _deckUpdateCards = new();
    public List<Card_ScrObj> deckUpdateCards => _deckUpdateCards;

    [ES3Serializable] private List<Card_ScrObj> _builtDeckCards = new();
    public List<Card_ScrObj> builtDeckCards => _builtDeckCards;


    // Data
    public List<Card_ScrObj> CardPool_Cards()
    {
        List<Card_ScrObj> poolCards = new();

        foreach (Card_ScrObj card in cardPool)
        {
            poolCards.Add(card);
        }
        return poolCards;
    }
    public List<Card_ScrObj> TargetGroup_CurrentCards(int groupIndex)
    {
        groupIndex = Mathf.Clamp(groupIndex, 0, _buildGroupCards.Count - 1);
        return _buildGroupCards[groupIndex];
    }


    public List<CardData> BuiltDeck_CardDatas()
    {
        List<CardData> datas = new();

        foreach (Card_ScrObj card in _builtDeckCards)
        {
            datas.Add(new(card));
        }
        return datas;
    }

    // todo check these two, no edits
    public void AddCard_toBuiltDeck(Card_ScrObj cardToAdd, int addAmount)
    {
        for (int i = 0; i < addAmount; i++)
        {
            _builtDeckCards.Add(cardToAdd);
        }
    }
    public void RemoveCard_fromBuiltDeck(Card_ScrObj cardToRemove, int removeAmount)
    {
        int removeCount = 0;

        for (int i = _builtDeckCards.Count - 1; i >= 0; i--)
        {
            if (_builtDeckCards.Contains(cardToRemove) == false) return;
            if (removeCount >= removeAmount) return;

            if (cardToRemove != _builtDeckCards[i]) continue;
            _builtDeckCards.RemoveAt(i);

            removeCount++;
        }
    }

    public bool DeckUpdate_NotComplete()
    {
        return _deckUpdateCompleteCount > 0;
    }

    public void Toggle_DeckUpdate(int updateCount)
    {
        _buildGroupCards.Clear();
        _deckUpdateCompleteCount = Mathf.Max(0, updateCount);
    }
    public bool Complete_DeckUpdate()
    {
        int updateCardsCount = _deckUpdateCards.Count;
        if (updateCardsCount < _deckUpdateCompleteCount) return false;

        for (int i = 0; i < updateCardsCount; i++)
        {
            _builtDeckCards.Add(_deckUpdateCards[i]);
        }

        _buildGroupCards.Clear();

        _deckUpdateCompleteCount = 0;
        _deckUpdateCards.Clear();

        return true;
    }


    // Debug
    public void Debug_BuiltDeckCards()
    {
        Dictionary<Card_ScrObj, int> cardCountData = new();

        for (int i = 0; i < _builtDeckCards.Count; i++)
        {
            Card_ScrObj card = _builtDeckCards[i];

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