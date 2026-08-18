using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, IInteractable
{
    [Space(10)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    [Space(20)]
    [SerializeField] private Animator_Controller _baseAnimator;
    public Animator_Controller baseAnimator => _baseAnimator;

    [SerializeField] private Animator_Controller _contentAnimator;
    public Animator_Controller contentAnimator => _contentAnimator;


    private CardData _data;
    public CardData data => _data;

    public Action OnSetData;

    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


    private bool _actionRunning;
    public bool actionRunning => _actionRunning;

    private Tile _targetingTile;
    public Tile targetingTile => _targetingTile;


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

    private EventBus_Controller _healthUpdateActionBus = new();
    public EventBus_Controller healthUpdateActionBus => _healthUpdateActionBus;

    private EventBus_Controller _deathUpdateActionBus = new();
    public EventBus_Controller deathUpdateActionBus => _deathUpdateActionBus;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        _data.currentData.OnCurrentHealthUpdate -= Handle_HealthUpdate;
    }
    

    // IInteractable
    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // Data
    public void Set_Data(CardData setData, Tile placeTile)
    {
        if (setData == null) return;

        Card_ScrObj loadCard = setData.cardScrObj;
        if (loadCard == null) return;

        _data = setData;
        _placedTile = placeTile;
        _contentSpriteRenderer.sprite = loadCard.contentSprite;

        _data.currentData.OnCurrentHealthUpdate += Handle_HealthUpdate;

        OnSetData?.Invoke();
    }
    public void Set_Data(Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(new CardData(setData), placeTile);
    }


    // Actions
    public IEnumerator RunActions_TargetingTiles()
    {
        _actionRunning = true;

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

                interactable.interactionData.Update_CurrentHealth(updateValue);

                yield return null;
                while (interactable.healthUpdating) yield return null;
            }

            yield return _afterTargetingSkillBus.RunSequential_DelayBusEvents();
        }

        yield return _afterUpdateSkillBus.RunSequential_DelayBusEvents();

        _tileTargeting.targetingTiles.Clear();
        _actionRunning = false;
        _targetingTile = null;

        yield break;
    }


    // Health
    private void Handle_HealthUpdate(int healthUpdateValue)
    {
        string animState = healthUpdateValue < 0 ? CardAnimation.Damage : CardAnimation.Heal;

        _baseAnimator.Play_State(animState);
        _contentAnimator.Play_State(animState);

        _healthUpdating = true;
        StartCoroutine(HealthUpdate_HandleDelay());
    }
    private bool Handle_Death()
    {
        if (_data.currentData.currentHealth > 0) return false;

        string deathStateAnim = CardAnimation.Destroy;

        _baseAnimator.Play_State(deathStateAnim);
        _contentAnimator.Play_State(deathStateAnim);

        _placedTile.Set_Occupant(null);
        return true;
    }

    private IEnumerator HealthUpdate_HandleDelay()
    {
        yield return null;
        while (_baseAnimator.CurrentState_Playing()) yield return null;

        yield return _healthUpdateActionBus.RunSequential_DelayBusEvents();

        if (Handle_Death())
        {
            yield return null;
            while (_baseAnimator.CurrentState_Playing()) yield return null;

            yield return _deathUpdateActionBus.RunSequential_DelayBusEvents();

            _actionRunning = false;
            _healthUpdating = false;

            GameManager.instance.cardManager.placedCards.Remove(this);
            Destroy(gameObject);
        }

        _healthUpdating = false;
        yield break;
    }
}