using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Activate Condition/Health Modify Success Target Count")]
public class HealthModifySuccess_TargetCount : ActivateCondition_ScrObj
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _overallCount;
    [SerializeField][Range(0, 100)] private int _runTimeCount;


    // abstract
    public override bool Activate_Available(Tile activatingTile)
    {
        IInteractable interactable = activatingTile.CurrentOccupant_Interactable();
        if (interactable == null) return false;

        // overall count
        int interactableCount = interactable.interactionData.healthModifySuccessCount;
        if (interactableCount < _overallCount) return false;

        if (activatingTile.currentOccupant.TryGetComponent(out IRunTimeAction runTimeAction) == false) return false;

        // runtime count
        int runTimeActionCount = runTimeAction.runTimeActionData.healthModifySuccessCount;
        if (runTimeActionCount < _runTimeCount) return false;

        return true;
    }
}
