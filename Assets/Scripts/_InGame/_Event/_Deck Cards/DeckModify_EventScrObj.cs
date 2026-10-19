using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Event/Deck Card Update")]
public class DeckModify_EventScrObj : Event_ScrObj
{
    [Space(40)]
    [SerializeField][Range(0, 100)] private int _updateAmount;
    public int updateAmount => _updateAmount;

    [SerializeField][Range(-100, 100)] private int _modifyAmount;
    public int modifyAmount => _updateAmount;

    [Space(10)]
    [SerializeField] private Card_ScrObj[] _updateCards;


    // Data
    public List<Card_ScrObj> Updated_Cards()
    {
        if (_modifyAmount >= 0)
        {
            List<Card_ScrObj> cardPool = new(GameManager.instance.deckBuildManager.data.cardPool);
            if (_updateCards.Length <= 0) return cardPool;
        }

        List<Card_ScrObj> updatedCards = new();
        int removeCount = Mathf.Abs(_modifyAmount);

        return updatedCards;
    }
}