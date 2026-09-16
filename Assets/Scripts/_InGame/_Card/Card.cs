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
    [SerializeField] private InteractionData_UpdateController _interactionDataUpdater;
    public InteractionData_UpdateController interactionDataUpdater => _interactionDataUpdater;

    [Space(20)]
    [SerializeField] private Animator_Controller _baseAnimator;
    public Animator_Controller baseAnimator => _baseAnimator;

    [SerializeField] private Animator_Controller _contentAnimator;
    public Animator_Controller contentAnimator => _contentAnimator;

    [Space(20)]
    [SerializeField] private CardSkill_TriggerData[] _skillDatas;
    public CardSkill_TriggerData[] skillDatas => _skillDatas;


    private GameObject _cardRootObject;

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
    public int targetingCount => _data.currentData.targetSelectCount;

    public bool Targeting_Available(Tile targetingTile)
    {
        int distance = Utility.Chebyshev_Distance(_placedTile.data.position, targetingTile.data.position);
        return distance <= _data.currentData.interactRange;
    }


    // MonoBehaviour
    private void OnDestroy()
    {
        UnRegister_SkillDatas();

        // from Set_Data
        _interactionDataUpdater.AfterDeathUpdate -= Remove_Data;
    }


    // Data
    public void Set_Data(GameObject cardRootObject, CardData setData, Tile placeTile)
    {
        _cardRootObject = cardRootObject;

        if (setData == null) return;

        Card_ScrObj loadCard = setData.cardScrObj;
        if (loadCard == null) return;

        _data = setData;
        _placedTile = placeTile;
        _contentSpriteRenderer.sprite = loadCard.contentSprite;

        _interactionDataUpdater.Set_Data(_data.currentData);
        _interactionDataUpdater.AfterDeathUpdate += Remove_Data;

        Register_SkillDatas();

        OnSetData?.Invoke();
    }
    public void Set_Data(GameObject cardRootObject, Card_ScrObj setData, Tile placeTile)
    {
        Set_Data(cardRootObject, new CardData(setData), placeTile);
    }

    private void Remove_Data()
    {
        GameManager.instance.cardManager.placedCards.Remove(this);
        Destroy(_cardRootObject);
    }


    public EventBus_Controller SkillTrigger_EventBus(CardSkillTrigger triggerType)
    {
        switch (triggerType)
        {
            case CardSkillTrigger.Place: return _placeUpdateActionBus;
            case CardSkillTrigger.PreUpdate: return _preUpdateSkillBus;
            case CardSkillTrigger.AfterUpdate: return _afterUpdateSkillBus;
            case CardSkillTrigger.PreTargeting: return _preTargetingSkillBus;
            case CardSkillTrigger.AfterTargeting: return _afterTargetingSkillBus;
            case CardSkillTrigger.HealthUpdate: return _interactionDataUpdater.healthUpdateActionBus;
            case CardSkillTrigger.Death: return _interactionDataUpdater.deathUpdateActionBus;
        }
        return null;
    }

    private void Register_SkillDatas()
    {
        for (int i = 0; i < _skillDatas.Length; i++)
        {
            CardSkill_TriggerData data = _skillDatas[i];

            EventBus_Controller triggerBus = SkillTrigger_EventBus(data.trigger);
            if (triggerBus == null) return;

            CardSkill[] triggerSkills = data.cardSkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                CardSkill skill = triggerSkills[j];
                if (skill == null) continue;

                triggerBus.Register(j, skill.Trigger_Skill);
                skill.Set_Data(this, data.trigger, data.target);
            }
        }
    }
    private void UnRegister_SkillDatas()
    {
        for (int i = 0; i < _skillDatas.Length; i++)
        {
            CardSkill_TriggerData data = _skillDatas[i];

            EventBus_Controller triggerBus = SkillTrigger_EventBus(data.trigger);
            if (triggerBus == null) return;

            CardSkill[] triggerSkills = data.cardSkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                CardSkill skill = triggerSkills[j];
                if (skill == null) continue;

                triggerBus.UnRegister(skill.Trigger_Skill);
            }
        }
    }


    // End Turn Action
    private void Reset_TileTargeting()
    {
        _targetingTile = null;
        _tileTargeting.targetingTiles.Clear();
    }

    public IEnumerator Run_EndTurnActions()
    {
        if (_data.currentData.Remove_State(InteractableState.Frozen))
        {
            Reset_TileTargeting();
            yield break;
        }

        _actionsRunning = true;
        yield return _preUpdateSkillBus.RunSequential_DelayBusEvents();

        if (_data.currentData.Remove_State(InteractableState.Frozen) == false)
        {
            List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);
            for (int i = 0; i < targetingTiles.Count; i++)
            {
                Tile tile = targetingTiles[i];
                _targetingTile = tile;

                yield return _preTargetingSkillBus.RunSequential_DelayBusEvents();

                IInteractable tileInteractable = tile.CurrentOccupant_Interactable();
                if (tileInteractable != null)
                {
                    InteractionData targetData = tileInteractable.interactionData;
                    if (targetData == null) continue;

                    int updateValue = targetData.currentHealth + _data.currentData.healthModifyValue;

                    // run health updating animation (animation is set relative to updateValue) ?
                    while (_baseAnimator.CurrentState_Playing()) yield return null;

                    targetData.Update_CurrentHealth(updateValue);

                    yield return null;
                    while (targetData.dataUpdating) yield return null;
                }

                yield return _afterTargetingSkillBus.RunSequential_DelayBusEvents();
            }
        }

        yield return _afterUpdateSkillBus.RunSequential_DelayBusEvents();

        Reset_TileTargeting();

        _actionsRunning = false;
        yield break;
    }
}