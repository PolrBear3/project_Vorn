using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;


    private CardData _data;
    public CardData data => _data;

    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        CardManager cardManager = GameManager.instance.cardManager;

        cardManager.OnCardAction -= Damage_TargetingTiles_Interactables;
        cardManager.OnCardAction -= _tileTargeting.targetingTiles.Clear;
    }


    // Data
    public void Set_Data(CardData setData, Tile placeTile)
    {
        if (setData == null) return;

        Card_ScrObj loadCard = setData.cardScrObj;
        if (loadCard == null) return;

        _data = setData;
        _placedTile = placeTile;
        _contentSpriteRenderer.sprite = loadCard.contentSprite;


        CardManager cardManager = GameManager.instance.cardManager;

        cardManager.OnCardAction += Damage_TargetingTiles_Interactables;
        cardManager.OnCardAction += _tileTargeting.targetingTiles.Clear;
    }
    public void Set_Data(Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(new CardData(setData), placeTile);
    }


    // Tile Targeting
    private void Damage_TargetingTiles_Interactables()
    {
        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];

            if (tile.currentOccupant == null) continue;
            if (tile.currentOccupant.TryGetComponent(out IInteractable interactable) == false) continue;

            InteractionData targetData = interactable.interactionData;
            int updateData = targetData.health + _data.currentData.healthModifyValue;

            interactable.interactionData.Update_CurrentHealth(updateData);
        }
    }
}