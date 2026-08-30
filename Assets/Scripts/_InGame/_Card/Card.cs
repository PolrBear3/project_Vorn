using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, IInteractable, ITileTargeting
{
    [Space(10)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    [Space(20)]
    [SerializeField] private Animator_Controller _baseAnimator;
    public Animator_Controller baseAnimator => _baseAnimator;

    [SerializeField] private Animator_Controller _contentAnimator;
    public Animator_Controller contentAnimator => _contentAnimator;

    [Space(20)]
    [SerializeField] private InteractableHealth_Controller _healthController;
    public InteractableHealth_Controller healthController => _healthController;


    private CardData _data;
    public CardData data => _data;

    public Action OnSetData;

    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


    private EventBus_Controller _placeUpdateActionBus = new();
    public EventBus_Controller placeUpdateActionBus => _placeUpdateActionBus;

    private EventBus_Controller _preUpdateSkillBus = new();
    public EventBus_Controller preUpdateSkillBus => _preUpdateSkillBus;

    private EventBus_Controller _afterUpdateSkillBus = new();
    public EventBus_Controller afterUpdateSkillBus => _afterUpdateSkillBus;

    private EventBus_Controller _preTargetingSkillBus = new();
    public EventBus_Controller preTargetingSkillBus => _preTargetingSkillBus;

    private EventBus_Controller _afterTargetingSkillBus = new();
    public EventBus_Controller afterTargetingSkillBus => _afterTargetingSkillBus;

    private Tile _targetingTile;
    public Tile targetingTile => _targetingTile;

    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;


    // IInteractable
    public InteractionData interactionData => _data.currentData;


    // ITileTargeting
    public Tile pivotTile => _placedTile;
    public TileTargeting_Data targetingData => _tileTargeting;

    public int targetingRange => _data.currentData.interactRange;
    public int targetingCount => _data.currentData.targetSelectCount;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        _healthController.AfterDeathUpdate -= Remove_Data;
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

        _healthController.Set_Data(_data.currentData);
        _healthController.AfterDeathUpdate += Remove_Data;

        OnSetData?.Invoke();
    }
    public void Set_Data(Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(new CardData(setData), placeTile);
    }

    private void Remove_Data()
    {
        GameManager.instance.cardManager.placedCards.Remove(this);
        Destroy(gameObject);
    }


    // End Turn Action
    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;

        yield return _preUpdateSkillBus.RunSequential_DelayBusEvents();

        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);
        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];
            _targetingTile = tile;

            yield return _preTargetingSkillBus.RunSequential_DelayBusEvents();

            GameObject tileOccupant = tile.currentOccupant;
            if (tileOccupant != null && tileOccupant.TryGetComponent(out IInteractable interactable))
            {
                InteractionData targetData = interactable.interactionData;
                if (targetData == null) continue;

                int updateValue = targetData.currentHealth + _data.currentData.healthModifyValue;

                // run health updating animation (animation is set relative to updateValue) ?
                while (_baseAnimator.CurrentState_Playing()) yield return null;

                targetData.Update_CurrentHealth(updateValue);

                yield return null;
                while (targetData.healthUpdating) yield return null;
            }

            yield return _afterTargetingSkillBus.RunSequential_DelayBusEvents();
        }

        yield return _afterUpdateSkillBus.RunSequential_DelayBusEvents();

        _tileTargeting.targetingTiles.Clear();
        _actionsRunning = false;
        _targetingTile = null;

        yield break;
    }
}