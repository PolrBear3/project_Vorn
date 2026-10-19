using System;
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
            Event_ScrObj toggleEvent = menu.Update_TargetEvent(targetEvent) ? targetEvent : null;

            menu.ToggleUpdate_TargetEvent(toggleEvent);
            menu.toggleController.Toggle(toggleEvent != null);
        }
    }
    public EventMenu Toggled_EventMenu()
    {
        for (int i = 0; i < _eventMenus.Length; i++)
        {
            EventMenu menu = _eventMenus[i];

            if (menu.targetEvent == null) continue;
            return menu;
        }
        return null;
    }

    public void Confirm_ToggledEventMenu()
    {
        EventMenu currentMenu = Toggled_EventMenu();

        if (currentMenu == null || currentMenu.OnConfirmAvailable?.Invoke() == false) return;
        currentMenu.Confirm();

        StageManager stageManager = GameManager.instance.stageManager;
        if (stageManager.Is_EventStage())
        {
            stageManager.ToggleQueued_EventStage();
            return;
        }
        Toggle_EventMenu(null);
    }
}