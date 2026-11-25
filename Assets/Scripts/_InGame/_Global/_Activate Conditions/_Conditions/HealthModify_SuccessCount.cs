using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Activate Condition/Health Modify Success Target Count")]
public class HealthModify_SuccessCount : ActivateCondition_ScrObj
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _successCount;


    // ActivateCondition_ScrObj
    public override bool Activate_Available(Tile activatingTile)
    {
        IInteractable interactable = activatingTile.CurrentOccupant_Interactable();
        if (interactable == null) return false;

        int modifyCount = interactable.interactionDataUpdater.targetData.healthModifyHistoryDatas.Count;

        return modifyCount >= _successCount;
    }
}
