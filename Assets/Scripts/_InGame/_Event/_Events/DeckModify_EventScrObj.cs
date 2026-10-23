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
    public int modifyAmount => _modifyAmount;

    [Space(20)]
    [SerializeField] private Card_ScrObj[] _modifyTargetCards;
    public Card_ScrObj[] modifyTargetCards => _modifyTargetCards;
}