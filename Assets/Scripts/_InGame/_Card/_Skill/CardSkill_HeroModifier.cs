using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_HeroModifier : CardSkill
{
    [Space(40)]
    [SerializeField][Range(-100, 100)] private int _maxManaModifyValue;
    [SerializeField][Range(-100, 100)] private int _currentManaModifyValue;

    [Space(10)]
    [SerializeField][Range(-10, 10)] private int _movementManaMultiplyModifyValue;


    // CardSkill
    public override IEnumerator Trigger_Skill()
    {
        List<IInteractable> currentTargetInteractables = CurrentTarget_Interactables();

        for (int i = 0; i < currentTargetInteractables.Count; i++)
        {
            if (currentTargetInteractables[i] is not Hero hero) continue;
            HeroData heroData = hero.data;

            heroData.Update_MaxManaCount(heroData.maxManaCount + _maxManaModifyValue);
            heroData.Update_CurrentManaCount(heroData.currentManaCount + _currentManaModifyValue);
            heroData.Modify_MovementMana_MultiplyValue(_movementManaMultiplyModifyValue);

            yield break;
        }
    }
}
