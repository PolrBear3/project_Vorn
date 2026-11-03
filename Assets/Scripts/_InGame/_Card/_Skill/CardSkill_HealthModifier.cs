using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_HealthModifier : CardSkill
{
    [Space(40)]
    [SerializeField][Range(0, 10)] private int _targetingCount;
    [SerializeField][Range(-100, 100)] private int _healthModifyValue;


    // CardSkill
    public override IEnumerator Trigger_Skill()
    {
        List<IInteractable> targetInteractables = CurrentTarget_Interactables(_targetingCount);

        for (int i = 0; i < targetInteractables.Count; i++)
        {
            InteractionData_UpdateController dataController = targetInteractables[i].interactionDataUpdater;
            int modifyValue = card.data.currentData.Modify_OtherData_CurrentHealth(dataController.targetData, _healthModifyValue);

            yield return dataController.Handle_HealthUpdate(modifyValue);
        }
        yield break;
    }
}