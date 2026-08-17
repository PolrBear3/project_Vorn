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
        int spawnCardsCount = _spawnCards.Length;

        if (spawnCardsCount <= 0) return null;
        return _spawnCards[Random.Range(0, spawnCardsCount)];
    }

    private IEnumerator SpawnCard_onTargetTile()
    {
        CardManager cardManager = GameManager.instance.cardManager;

        Tile spawnTile = cardManager.ActionRunningCard_TargetingTile();
        cardManager.PlaceCard_OnTile(new(SpawnCard()), spawnTile);

        yield break;
    }
}