using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Event/Deck Card Update")]
public class DeckCardUpdate_EventScrObj : Event_ScrObj
{
    [Space(40)]
    [SerializeField][Range(-100, 100)]  private int _updateAmount;
    [SerializeField] private Card_ScrObj[] _targetCards;
}