using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Activate Condition/Interact Range Contains Target")]
public class InteractRange_ContainsTarget : ActivateCondition_ScrObj
{
    [Space(20)]
    [SerializeField] private bool _containHero;
    [Space(10)]
    [SerializeField] private Card_ScrObj[] _containCards;
    [SerializeField] private Enemy_ScrObj[] _containEnemies;


    // Data
    private int CurrentInteractable_InteractRange(Tile currentTile)
    {
        if (currentTile == null) return 0;

        IInteractable interactable = currentTile.CurrentOccupant_Interactable();
        if (interactable == null) return 0;

        return interactable.interactionData.interactRange;
    }

    private bool HeroContain_Check(List<Tile> rangeTiles)
    {
        if (_containHero == false) return true;

        Hero spawnedHero = GameManager.instance.heroManager.spawnedHero;
        if (spawnedHero == null) return false;

        return rangeTiles.Contains(spawnedHero.movement.currentTile);
    }
    private bool CardsContain_Check(List<Tile> rangeTiles)
    {
        if (_containCards.Length <= 0) return true;

        CardManager cardManager = GameManager.instance.cardManager;

        for (int i = 0; i < rangeTiles.Count; i++)
        {
            Card placedCard = cardManager.PlacedCard(rangeTiles[i]);
            if (placedCard == null) continue;

            Card_ScrObj cardToCheck = placedCard.data.cardScrObj;

            for (int j = 0; j < _containCards.Length; j++)
            {
                if (cardToCheck != _containCards[j]) continue;
                return true;
            }
        }
        return false;
    }
    private bool EnemiesContain_Check(List<Tile> rangeTiles)
    {
        return true;
    }


    // abstract
    public override bool Activate_Available(Tile activatingTile)
    {
        int interactRange = CurrentInteractable_InteractRange(activatingTile);
        List<Tile> rangeTiles = GameManager.instance.tileManager.Distanced_Tiles(activatingTile, interactRange);

        if (HeroContain_Check(rangeTiles) == false) return false;
        if (CardsContain_Check(rangeTiles) == false) return false;
        if (EnemiesContain_Check(rangeTiles) == false) return false;

        return true;
    }
}