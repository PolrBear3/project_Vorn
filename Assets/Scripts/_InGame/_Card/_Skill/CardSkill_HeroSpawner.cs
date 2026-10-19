using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_HeroSpawner : CardSkill
{
    // abstract
    public override IEnumerator Trigger_Skill()
    {
        GameManager manager = GameManager.instance;

        Hero_ScrObj selectedHero = manager.heroManager.data.selectedHero;
        if (selectedHero == null) yield break;

        Tile currentTile = card.placedTile;
        Vector2 spawnPosition = (Vector2)currentTile.transform.position + selectedHero.spawnOffset;

        GameObject spawnHeroObj = Instantiate(selectedHero.spawnPrefab, spawnPosition, Quaternion.identity);

        if (spawnHeroObj.TryGetComponent(out Hero spawnHero) == false)
        {
            Destroy(spawnHeroObj);
            yield break;
        }

        currentTile.Set_Occupant(null);
        currentTile.Set_Occupant(spawnHeroObj);

        spawnHero.Set_Data(selectedHero); // set data before tracking hero
        manager.heroManager.Track_SpawnedHero(spawnHero); // needs interaction data set for updating

        spawnHero.movement.Set_CurrentTile(currentTile);
        spawnHero.animator.Play_State(OccupantAnimation.Set);

        yield return null;
        while (spawnHero.animator.CurrentState_Playing()) yield return null;

        yield break;
    }
}