using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage/Event Stage")]
public class EventStage_ScrObj : Stage_ScrObj
{
    [Space(40)]
    [SerializeField] private Event_ScrObj[] _sequntialEvents;
    [SerializeField] private Event_ScrObj[] _randomSelectEvents;


    // Data
    public List<Event_ScrObj> Combined_Events()
    {
        List<Event_ScrObj> combinedEvents = new();

        for (int i = 0; i < _sequntialEvents.Length; i++)
        {
            combinedEvents.Add(_sequntialEvents[i]);
        }

        int randomSelectEventsCount = _randomSelectEvents.Length;
        if (randomSelectEventsCount <= 0) return combinedEvents;

        Event_ScrObj selectedEvent = _randomSelectEvents[Random.Range(0, randomSelectEventsCount)];

        combinedEvents.Add(selectedEvent);
        return combinedEvents;
    }
}