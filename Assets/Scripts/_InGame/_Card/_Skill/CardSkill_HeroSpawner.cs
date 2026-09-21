using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_HeroSpawner : CardSkill
{
    // from abstract Trigger_Skill
    public override IEnumerator Trigger_Skill()
    {
        GameManager manager = GameManager.instance;

        Hero_ScrObj currentHero = manager.currentGameData.hero;
        if (currentHero == null) yield break;

        Tile currentTile = card.placedTile;
        Vector2 spawnPosition = (Vector2)currentTile.transform.position + currentHero.spawnOffset;

        GameObject spawnHeroObj = Instantiate(currentHero.spawnPrefab, spawnPosition, Quaternion.identity);

        if (spawnHeroObj.TryGetComponent(out Hero spawnHero) == false)
        {
            Destroy(spawnHeroObj);
            yield break;
        }

        currentTile.Set_Occupant(null);
        currentTile.Set_Occupant(spawnHeroObj);

        spawnHero.Set_Data(currentHero); // set data before tracking hero
        manager.heroManager.Track_CurrentHero(spawnHero); // needs interaction data set for updating

        spawnHero.movement.Set_CurrentTile(currentTile);
        spawnHero.animator.Play_State(OccupantAnimation.Set);

        yield return null;
        while (spawnHero.animator.CurrentState_Playing()) yield return null;

        yield break;
    }
}
