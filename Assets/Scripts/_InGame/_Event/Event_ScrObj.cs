using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField][TextArea(3, 10)] private string _eventDescription;
    public string eventDescription => _eventDescription;
}
