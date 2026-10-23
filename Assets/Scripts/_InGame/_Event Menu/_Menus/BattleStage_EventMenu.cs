using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStage_EventMenu : EventMenu
{
    // EventMenu
    public override void AutoUpdate_forConfirm()
    {

    }
    public override void Confirm()
    {

    }

    public override bool Update_TargetEvent(Event_ScrObj updateEvent)
    {
        if (updateEvent is not BattleStage_EventScrObj battleStageEvent) return false;
        BattleStage_ScrObj[] battleStages = battleStageEvent.battleStages;

        int battleStageCount = battleStages.Length;
        if (battleStageCount <= 0) return false;

        BattleStage_ScrObj updateBattleStage = battleStages[UnityEngine.Random.Range(0, battleStages.Length)];
        GameManager.instance.stageManager.stageMap.data.Update_CurrentStageData(new(updateBattleStage));

        return true;
    }
}
