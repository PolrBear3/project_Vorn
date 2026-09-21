using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Event/Deck Card Remove")]
public class DeckCardRemove_EventScrObj : Event_ScrObj
{
    [Space(40)]
    [SerializeField][Range(0, 100)]  private int _removeAmount;
    [SerializeField] private Card_ScrObj[] _removeCards;
}