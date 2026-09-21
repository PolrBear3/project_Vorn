using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage/Event Stage")]
public class EventStage_ScrObj : Stage_ScrObj
{
    [Space(40)]
    [SerializeField] private Event_ScrObj[] _sequntialEvents;
    [SerializeField] private Event_ScrObj[] _randomSelectEvents;
}