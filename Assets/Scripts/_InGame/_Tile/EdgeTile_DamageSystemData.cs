using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTile_DamageSystemData
{
    private Dictionary<IInteractable, int> _stackDamageDatas = new();
    public Dictionary<IInteractable, int> stackDamageDatas => _stackDamageDatas;


    // Data
    public int Current_StackDamageCount(IInteractable targetInteractable)
    {
        if (targetInteractable == null) return 0;
        if (_stackDamageDatas.ContainsKey(targetInteractable) == false) return 0;

        return _stackDamageDatas[targetInteractable];
    }

    public void Update_StackDamage(IInteractable targetInteractable)
    {
        if (targetInteractable == null) return;

        if (targetInteractable.interactionData.currentHealth <= 0)
        {
            _stackDamageDatas.Remove(targetInteractable);
            return;
        }

        if (_stackDamageDatas.ContainsKey(targetInteractable))
        {
            _stackDamageDatas[targetInteractable] ++;
            return;
        }
        _stackDamageDatas[targetInteractable] = 1;
    }
}