using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_Spawner : CardSkill
{
    [Space(40)]
    [SerializeField][Range(0, 10)] private int _spawnCount;
    [SerializeField] private Card_ScrObj[] _spawnCards;

    private const float _spawnDelayTime = 0.4f;


    // data
    private Card_ScrObj SpawnCard()
    {
        int spawnCardsCount = _spawnCards.Length;

        if (spawnCardsCount <= 0) return null;
        return _spawnCards[Random.Range(0, spawnCardsCount)];
    }


    // CardSkill
    public override IEnumerator Trigger_Skill()
    {
        CardManager cardManager = GameManager.instance.cardManager;
        List<Tile> spawnTiles = SpawnTarget_Tiles(_spawnCount);

        for (int i = 0; i < spawnTiles.Count; i++)
        {
            cardManager.PlaceCard_OnTile(new(SpawnCard()), spawnTiles[i]);
            yield return new WaitForSeconds(_spawnDelayTime);
        }
        yield break;
    }
}