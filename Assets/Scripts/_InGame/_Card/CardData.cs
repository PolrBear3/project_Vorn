using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    private Card_ScrObj _cardScrObj;
    public Card_ScrObj cardScrObj => _cardScrObj;

    private InteractionData _currentData;
    public InteractionData currentData => _currentData;


    // New
    public CardData(Card_ScrObj setCard)
    {
        _cardScrObj = setCard;
        _currentData = setCard.interactionData;
    }
}
