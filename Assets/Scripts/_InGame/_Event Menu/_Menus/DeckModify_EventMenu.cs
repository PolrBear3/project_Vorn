using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu
{
    // MonoBehaviour
    private void Awake()
    {
        // toggleController.OnToggle += ;
    }

    private void OnDestroy()
    {
        // toggleController.OnToggle -= ;
    }


    // EventMenu abstract
    public override bool Toggle_Available(Event_ScrObj checkEvent)
    {
        return checkEvent is DeckModify_EventScrObj;
    }
}