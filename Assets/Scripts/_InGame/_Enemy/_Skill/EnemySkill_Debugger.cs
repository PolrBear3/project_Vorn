using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill_Debugger : EnemySkill
{
    [Space(20)]
    [SerializeField] private int _delayTime;
    [SerializeField][TextArea(3, 10)] private string _debugString;
    
    public override IEnumerator Trigger_Skill()
    {
        yield return new WaitForSeconds(_delayTime);
        
        Debug.Log(_debugString);
        yield break;
    }
}
