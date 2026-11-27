using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill_RunTimeData : Skill_RunTimeData
{
    private int _activationCoolTime;
    public int activationCoolTime => _activationCoolTime;


    // Data Update
    public void Update_ActivationCoolTime(int updateCoolTime)
    {
        _activationCoolTime = updateCoolTime;
    }
    public bool On_ActivationCoolTime(int setCoolTime)
    {
        if (setCoolTime <= 0)
        {
            _activationCoolTime = 0;
            return false;
        }

        if (_activationCoolTime < setCoolTime)
        {
            _activationCoolTime++;
            return true;
        }

        _activationCoolTime = 0;
        return false;
    }
}
