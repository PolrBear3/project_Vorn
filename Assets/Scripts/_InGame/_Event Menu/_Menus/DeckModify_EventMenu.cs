using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu
{
    // abstract
    public override bool Toggle_Available(Event_ScrObj checkEvent)
    {
        return checkEvent is DeckModify_EventScrObj;
    }

    public override void Toggle(Event_ScrObj targetModifyEvent)
    {
        Debug.Log(targetModifyEvent);
    }
}