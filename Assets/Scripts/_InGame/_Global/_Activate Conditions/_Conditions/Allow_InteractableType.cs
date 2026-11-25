using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Activate Condition/Interactable Type")]
public class Allow_InteractableType : ActivateCondition_ScrObj
{
    [Space(20)]
    [SerializeField] private bool _allowFriendly;
    [SerializeField] private bool _allowEnemy;


    // ActivateCondition_ScrObj
    public override bool Activate_Available(Tile activatingTile)
    {
        if (activatingTile == null) return false;

        IInteractable target = activatingTile.CurrentOccupant_Interactable();

        if (target is Card || target is Hero) return _allowFriendly;
        if (target is Enemy) return _allowEnemy;

        return false;
    }
}
