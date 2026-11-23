using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Activate Condition/Health Updated Success")]
public class HealthUpdatedSuccess_TargetCount : ActivateCondition_ScrObj
{
    [Space(20)]
    [SerializeField][Range(-100, 100)] private int _successValue;


    // ActivateCondition_ScrObj
    public override bool Activate_Available(Tile activatingTile)
    {
        IInteractable interactable = activatingTile.CurrentOccupant_Interactable();
        if (interactable == null) return false;

        InteractionData data = interactable.interactionDataUpdater.targetData;
        if (data.currentHealth <= 0) return false;

        int recentHealthModifiedValue = data.recentHealthModifiedValue;

        if (_successValue < 0 && recentHealthModifiedValue <= _successValue) return true;
        if (_successValue > 0 && recentHealthModifiedValue >= _successValue) return true;

        return false;
    }
}
