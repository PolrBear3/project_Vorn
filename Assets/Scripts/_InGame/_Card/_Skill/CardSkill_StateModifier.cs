using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_StateModifier : CardSkill
{
    [Space(40)]
    [SerializeField][Range(0, 100)] private int _targetingCount;

    [Space(20)]
    [SerializeField] private InteractableState[] _statesToAdd;
    [SerializeField] private InteractableState[] _statesToRemove;


    // CardSkill
    public override IEnumerator Trigger_Skill() // todo animation for state update not working
    {
        List<IInteractable> targetInteractables = CurrentTarget_Interactables(_targetingCount);

        for (int i = 0; i < targetInteractables.Count; i++)
        {
            InteractionData_UpdateController dataController = targetInteractables[i].interactionDataUpdater;
            InteractionData targetData = dataController.targetData;

            for (int j = 0; j < _statesToAdd.Length; j++)
            {
                InteractableState stateToAdd = _statesToAdd[j];

                targetData.Add_State(stateToAdd);
                yield return dataController.Handle_StateUpdate(stateToAdd);
            }

            for (int j = 0; j < _statesToRemove.Length; j++)
            {
                InteractableState stateToRemove = _statesToRemove[j];

                targetData.Remove_State(stateToRemove);
                yield return dataController.Handle_StateUpdate(stateToRemove);
            }
        }
    }
}
