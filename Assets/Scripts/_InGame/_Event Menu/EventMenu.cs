using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventMenu : MonoBehaviour
{
    [SerializeField] private UIPanel_ToggleController _toggleController;
    public UIPanel_ToggleController toggleController => _toggleController;

    private Event_ScrObj _targetEvent;
    public Event_ScrObj targetEvent => _targetEvent;


    public abstract bool Toggle_Available(Event_ScrObj checkEvent);

    public void Update_TargetEvent(Event_ScrObj updateEvent)
    {
        _targetEvent = updateEvent;
    }
}