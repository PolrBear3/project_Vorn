using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    [Space(20)]
    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;


    private CardData _data;
    public CardData data => _data;

    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;

    private Tile _targetingTile;
    public Tile targetingTile => _targetingTile;


    public Action OnSetData;

    private EventBus_Controller _placeUpdateActionBus = new();
    public EventBus_Controller placeUpdateActionBus => _placeUpdateActionBus;

    private EventBus_Controller _preUpdateSkillBus = new();
    public EventBus_Controller preUpdateSkillBus => _preUpdateSkillBus;

    private EventBus_Controller _afterUpdateSkillBus = new();
    public EventBus_Controller afterUpdateSkillBus => _afterUpdateSkillBus;


    // Data
    public void Set_Data(CardData setData, Tile placeTile)
    {
        if (setData == null) return;

        Card_ScrObj loadCard = setData.cardScrObj;
        if (loadCard == null) return;

        _data = setData;
        _placedTile = placeTile;
        _contentSpriteRenderer.sprite = loadCard.contentSprite;

        OnSetData?.Invoke();
    }
    public void Set_Data(Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(new CardData(setData), placeTile);
    }


    // Action
    public IEnumerator RunActions_TargetingTiles()
    {
        _actionsRunning = true;

        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];
            _targetingTile = tile;

            GameObject tileOccupant = tile.currentOccupant;

            StartCoroutine(_preUpdateSkillBus.SequentialDelayBus_RunUpdate());
            while (_preUpdateSkillBus.delayBusRunning) yield return null;

            if (tileOccupant != null && tileOccupant.TryGetComponent(out IInteractable interactable))
            {
                InteractionData targetData = interactable.interactionData;
                if (targetData == null) continue;

                int updateValue = targetData.currentHealth + _data.currentData.healthModifyValue;

                // run health updating animation (animation is set relative to updateValue) ?
                while (_animator.CurrentState_Playing()) yield return null;

                interactable.interactionData.Update_CurrentHealth(updateValue);

                yield return null;
                while (interactable.healthUpdating) yield return null;
            }

            StartCoroutine(_afterUpdateSkillBus.SequentialDelayBus_RunUpdate());
            while (_afterUpdateSkillBus.delayBusRunning) yield return null;
        }
        _tileTargeting.targetingTiles.Clear();

        _actionsRunning = false;
        _targetingTile = null;

        yield break;
    }
}