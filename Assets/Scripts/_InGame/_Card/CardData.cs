using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    [ES3Serializable] private Card_ScrObj _cardScrObj;
    public Card_ScrObj cardScrObj => _cardScrObj;

    [ES3Serializable] private InteractionData _currentData;
    public InteractionData currentData => _currentData;

    [ES3Serializable] private int _currentManaPrice;
    public int currentManaPrice => _currentManaPrice;


    // New
    public CardData(Card_ScrObj setCard)
    {
        _cardScrObj = setCard;
        _currentManaPrice = setCard.manaCost;

        if (setCard == null) return;
        _currentData = new(setCard.interactionData);
    }
}
