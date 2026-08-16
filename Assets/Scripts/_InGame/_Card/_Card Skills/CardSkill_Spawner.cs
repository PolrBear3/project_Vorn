using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_Spawner : CardSkill
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _spawnCards;


    // MonoBehaviour
    private void Awake()
    {
        card.OnSetData += Set_Data;
    }
    
    private void OnDestroy()
    {
        card.OnSetData -= Set_Data;


        // from Set_Data
        card.afterUpdateSkillBus.UnRegister(SpawnCard_onTargetTile);
    }


    // Data
    private void Set_Data()
    {
        card.afterUpdateSkillBus.Register(0, SpawnCard_onTargetTile);
    }


    // Spawn
    private Card_ScrObj SpawnCard()
    {
        return null;
    }

    private IEnumerator SpawnCard_onTargetTile()
    {
        Debug.Log(GameManager.instance.cardManager.ActionRunningCard_TargetingTile());
        yield break;
    }
}