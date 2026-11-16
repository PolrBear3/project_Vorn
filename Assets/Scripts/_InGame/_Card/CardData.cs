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


    private bool _drawPrioritized;
    public bool drawPrioritized => _drawPrioritized;

    private bool _isGenerated;
    public bool isGenerated => _isGenerated;


    // New
    public CardData(Card_ScrObj setCard, bool isGeneratedCard)
    {
        if (setCard == null) return;

        _cardScrObj = setCard;
        _currentData = new(setCard.interactionData);

        _currentManaPrice = setCard.manaCost;

        _drawPrioritized = setCard.drawPrioritized;
        _isGenerated = isGeneratedCard;
    }

    public CardData(CardData loadData)
    {
        _cardScrObj = loadData._cardScrObj;
        _currentData = new(loadData._currentData);

        _currentManaPrice = loadData._currentManaPrice;

        _drawPrioritized = loadData._drawPrioritized;
        _isGenerated = loadData._isGenerated;
    }


    // Data
    public void Toggle_DrawPrioritize(bool toggle)
    {
        _drawPrioritized = toggle;
    }
}
