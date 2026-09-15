using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_Spawner : CardSkill
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _spawnCards;


    // Spawn
    private Card_ScrObj SpawnCard()
    {
        int spawnCardsCount = _spawnCards.Length;

        if (spawnCardsCount <= 0) return null;
        return _spawnCards[Random.Range(0, spawnCardsCount)];
    }
    private List<Tile> CardSpawn_TargetTiles()
    {
        List<Tile> targetTiles = new();

        Tile placedTile = card.placedTile;
        int interactRange = card.data.currentData.interactRange;

        List<Tile> targetingTiles = new(card.tileTargeting.recentTargetingTiles);
        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(placedTile, interactRange);

        switch (currentTarget, currentTrigger)
        {
            case (CardSkillTarget.ActionTargetingTile, CardSkillTrigger.HealthUpdate): return targetingTiles;
            case (CardSkillTarget.ActionTargetingTile, CardSkillTrigger.Death): return targetingTiles;

            case (CardSkillTarget.CurrentTile, _):
                targetTiles.Add(placedTile);
                break;

            case (CardSkillTarget.ActionTargetingTile, _):
                targetTiles.Add(card.targetingTile);
                break;

            case (CardSkillTarget.InteractRangeTiles, _): return new(interactRangeTiles);

            case (CardSkillTarget.InteractRangeTile, _):

                for (int i = interactRangeTiles.Count - 1; i >= 0; i--)
                {
                    if (interactRangeTiles[i].currentOccupant == null) continue;
                    interactRangeTiles.RemoveAt(i);
                }

                int remainingTileCount = interactRangeTiles.Count;
                if (remainingTileCount <= 0) return new(interactRangeTiles);

                targetTiles.Add(interactRangeTiles[Random.Range(0, remainingTileCount)]);
                break;
        }
        return targetTiles;
    }


    // from abstract Trigger_Skill
    public override IEnumerator Trigger_Skill()
    {
        CardManager cardManager = GameManager.instance.cardManager;
        List<Tile> spawnTiles = CardSpawn_TargetTiles();

        for (int i = 0; i < spawnTiles.Count; i++)
        {
            cardManager.PlaceCard_OnTile(new(SpawnCard()), spawnTiles[i]);
        }
        yield break;
    }
}