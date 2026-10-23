using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Event/Spawn to Battle Stage")]
public class BattleStage_EventScrObj : Event_ScrObj
{
    [Space(40)]
    [SerializeField] private BattleStage_ScrObj[] _battleStages;
    public BattleStage_ScrObj[] battleStages => _battleStages;
}
