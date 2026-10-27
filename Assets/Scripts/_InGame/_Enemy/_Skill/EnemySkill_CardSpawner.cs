using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill_CardSpawner : EnemySkill
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _spawnCount;
    [SerializeField] private Card_ScrObj[] _spawnCards;


    // data
    private Card_ScrObj SpawnCard()
    {
        int spawnCardsCount = _spawnCards.Length;

        if (spawnCardsCount <= 0) return null;
        return _spawnCards[Random.Range(0, spawnCardsCount)];
    }


    // EnemySkill
    public override IEnumerator Trigger_Skill()
    {
        CardManager cardManager = GameManager.instance.cardManager;
        List<Tile> spawnTiles = SpawnTarget_Tiles(_spawnCount);

        for (int i = 0; i < spawnTiles.Count; i++)
        {
            cardManager.PlaceCard_OnTile(new(SpawnCard()), spawnTiles[i]);
        }
        yield break;
    }
}
