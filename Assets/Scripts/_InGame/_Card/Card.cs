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
        EventBus_Controller cardActionBus = GameManager.instance.cardManager.cardActionBus;

        cardActionBus.UnRegister(UpdateHealth_TargetingInteractables);
        // cardActionBus.UnRegister(UpdateSkills_TargetingTiles);
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


        EventBus_Controller cardActionBus = GameManager.instance.cardManager.cardActionBus;

        cardActionBus.Register(0, UpdateHealth_TargetingInteractables);
        // cardActionBus.Register(0, UpdateSkills_TargetingTiles);
    }
    public void Set_Data(Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(new CardData(setData), placeTile);
    }


    // Tile Targeting
    private IEnumerator UpdateHealth_TargetingInteractables()
    {
        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];
            GameObject tileOccupant = tile.currentOccupant;

            if (tileOccupant == null) continue;
            if (tileOccupant.TryGetComponent(out IInteractable interactable) == false) continue;

            InteractionData targetData = interactable.interactionData;
            if (targetData == null) continue;

            int updateData = targetData.health + _data.currentData.healthModifyValue;
            interactable.interactionData.Update_CurrentHealth(updateData);

            yield return null;
            while (interactable.healthUpdating) yield return null;
        }

        _tileTargeting.targetingTiles.Clear();
        yield break;
    }

    private IEnumerator UpdateSkills_TargetingTiles()
    {
        Debug.Log("UpdateSkills_TargetingTiles");
        yield break;
    }
}