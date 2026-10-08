using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMenu_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventMenu[] _eventMenus;
    public EventMenu[] eventMenus => _eventMenus;

    public Func<bool> ConfirmAvailable;
    public Action OnConfirm;


    // Main
    public void Toggle_EventMenu(Event_ScrObj targetEvent)
    {
        for (int i = 0; i < _eventMenus.Length; i++)
        {
            EventMenu menu = _eventMenus[i];
            Event_ScrObj toggleEvent = menu.Toggle_Available(targetEvent) ? targetEvent : null;

            menu.Update_TargetEvent(toggleEvent);
            menu.toggleController.Toggle(toggleEvent != null);
        }
        GameManager.instance.stageManager.Toggle_BattleStage();
    }

    public void Confirm_CurrentEventMenu()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        if (stageManager.Is_EventStage())
        {
            stageManager.ToggleQueued_EventStage();
            return;
        }
        Toggle_EventMenu(null);
    }
}
