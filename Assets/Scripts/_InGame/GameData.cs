using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [SerializeField] private Stage_ScrObj _stage;
    public Stage_ScrObj stage => _stage;

    [SerializeField] private Hero_ScrObj _hero;
    public Hero_ScrObj hero => _hero;

    [Space(10)]
    [SerializeField] private List<Card_ScrObj> _deckCards;
    public List<Card_ScrObj> deckCards => _deckCards;


    // Data
    public List<CardData> DeckCard_Datas()
    {
        List<CardData> cardDatas = new();

        foreach (Card_ScrObj card in _deckCards)
        {
            cardDatas.Add(new(card));
        }
        return cardDatas;
    }
}
