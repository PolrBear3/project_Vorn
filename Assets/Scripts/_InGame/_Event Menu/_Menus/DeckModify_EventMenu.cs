using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu
{
    // MonoBehaviour
    private void Awake()
    {
        toggleController.OnToggle += Toggle_Debug;
    }

    private void OnDestroy()
    {
        toggleController.OnToggle -= Toggle_Debug;
    }


    // EventMenu abstract
    public override bool Toggle_Available(Event_ScrObj checkEvent)
    {
        return checkEvent is DeckModify_EventScrObj;
    }

    private void Toggle_Debug(bool toggle)
    {
        if (toggle == false) return;

        StageMap_Data data = GameManager.instance.stageManager.stageMap.data;
        Debug.Log(data.Current_StageData().stage + " " + data.Current_StageData().queueEvents.Count);
    }
}