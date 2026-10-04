using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMenu_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventMenu[] _eventMenus;
    public EventMenu[] eventMenus => _eventMenus;


    // Main
    public void Toggle_EventMenu(Event_ScrObj targetEvent)
    {
        for (int i = 0; i < _eventMenus.Length; i++)
        {
            EventMenu menu = _eventMenus[i];

            if (menu.Toggle_Available(targetEvent) == false) continue;
            menu.Toggle(targetEvent);
        }
    }
}
