using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    private Card_ScrObj _cardScrObj;
    public Card_ScrObj cardScrObj => _cardScrObj;

    private InteractionData _currentData;
    public InteractionData currentData => _currentData;

    private int _currentMana;
    public int currentMana => _currentMana;


    // New
    public CardData(Card_ScrObj setCard)
    {
        _cardScrObj = setCard;

        _currentData = setCard.interactionData;
        _currentMana = setCard.mana;
    }
}
