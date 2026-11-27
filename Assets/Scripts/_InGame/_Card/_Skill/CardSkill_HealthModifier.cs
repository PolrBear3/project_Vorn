using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_HealthModifier : CardSkill
{
    [Space(40)]
    [SerializeField][Range(0, 10)] private int _targetingCount;
    [SerializeField][Range(-100, 100)] private int _healthModifyValue;


    // CardSkill
    public override IEnumerator Activate_Skill()
    {
        InteractionData currentCardData = card.data.currentData;
        List<IInteractable> targetInteractables = CurrentTarget_Interactables(_targetingCount);

        for (int i = 0; i < targetInteractables.Count; i++)
        {
            InteractionData_UpdateController targetController = targetInteractables[i].interactionDataUpdater;
            InteractionData targetData = targetController.targetData;

            targetData.Update_CurrentHealth(targetData.currentHealth + _healthModifyValue, out int modifiedValue);
            currentCardData.Track_HealthModifyHistory(modifiedValue);

            yield return targetController.Handle_StateUpdate();
            yield return targetController.Handle_HealthUpdate(modifiedValue);
        }
    }
}