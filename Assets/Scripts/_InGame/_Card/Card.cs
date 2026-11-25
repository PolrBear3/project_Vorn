using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, ISubscriptionHandler, ITileTargeting, IInteractable
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    [Space(20)]
    [SerializeField] private InteractionData_UpdateController _interactionDataUpdater;
    public InteractionData_UpdateController interactionDataUpdater => _interactionDataUpdater;

    [SerializeField] private HealthModify_Indicator _modifyIndicator;
    public HealthModify_Indicator modifyIndicator => _modifyIndicator;

    [Space(20)]
    [Tooltip("Condition for starting the action.")]
    [SerializeField] private ActivateCondition_ScrObj[] _currentTileConditions;
    [Tooltip("Condition for target selected tiles.")]
    [SerializeField] private ActivateCondition_ScrObj[] _eachTargetingTileConditions;

    [Space(20)]
    [SerializeField] private CardSkill_TriggerData[] _skillDatas;


    private CardData _data;
    public CardData data => _data;

    public Action OnSetData;


    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;

    private Tile _actionTargetingTile;
    public Tile actionTargetingTile => _actionTargetingTile;


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
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();
        EventBus_GlobalController.UnRegister(this);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        _placeUpdateActionBus.Register(0, _interactionDataUpdater.Handle_StateUpdate);
        _interactionDataUpdater.OnPreDeathActionDataUpdate += Remove_Data;
    }

    public void UnSubscribe_All()
    {
        UnRegister_SkillDatas();

        _placeUpdateActionBus.UnRegister(_interactionDataUpdater.Handle_StateUpdate);
        _interactionDataUpdater.OnPreDeathActionDataUpdate -= Remove_Data;
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

        _interactionDataUpdater.Set_Data(_data.currentData);

        Subscribe_All();
        Register_SkillDatas();

        OnSetData?.Invoke();

        _modifyIndicator.Update_ModifyValue(_data.currentData.healthModifyValue);
        _modifyIndicator.Toggle_ValueBox(false);
    }
    private void Remove_Data()
    {
        GameManager manager = GameManager.instance;

        manager.deckBuildManager.data.RemoveCard_fromBuiltDeck(_data.isGenerated ? null : _data.cardScrObj, 1);
        manager.cardManager.placedCards.Remove(this);

        _placedTile.Update_CurrentOccupant(null);
    }


    // Skill
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
            if (triggerBus == null) continue;

            CardSkill[] triggerSkills = data.cardSkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                CardSkill skill = triggerSkills[j];
                if (skill == null) continue;

                triggerBus.Register(j, skill.Run_SkillTrigger);
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
            if (triggerBus == null) continue;

            CardSkill[] triggerSkills = data.cardSkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                CardSkill skill = triggerSkills[j];
                if (skill == null) continue;

                triggerBus.UnRegister(skill.Run_SkillTrigger);
            }
        }
    }


    // End Turn Action
    private void Reset_ActionDatas()
    {
        _tileTargeting.targetingTiles.Clear();
        _actionTargetingTile = null;

        _modifyIndicator.Toggle_ValueBox(false);
    }

    private IEnumerator Run_TileTargeting_HealthModify()
    {
        InteractionData currentdata = _interactionDataUpdater.targetData;
        int modifyValue = currentdata.healthModifyValue;

        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];
            if (CustomData_Utility.AllConditions_ActivateAvailable(tile, _eachTargetingTileConditions) == false) continue;

            _actionTargetingTile = tile;

            yield return _preTargetingSkillBus.RunSequential_DelayBusEvents();

            IInteractable targetInteractable = tile.CurrentOccupant_Interactable();
            InteractionData_UpdateController targetInteractableController = targetInteractable?.interactionDataUpdater;

            if (targetInteractableController != null)
            {
                InteractionData targetData = targetInteractableController.targetData;

                targetInteractableController.targetData.Update_CurrentHealth(targetData.currentHealth + modifyValue, out int modifiedValue);
                currentdata.Track_HealthModifyHistory(modifiedValue);

                _modifyIndicator.Toggle_ValueBox(modifiedValue != 0);

                yield return targetInteractableController.Handle_StateUpdate();
                yield return targetInteractableController.Handle_HealthUpdate(modifiedValue);
            }

            yield return _afterTargetingSkillBus.RunSequential_DelayBusEvents();
        }
    }
    public IEnumerator Run_EndTurnActions()
    {
        InteractionData currentCardData = _data.currentData;

        // check death before action
        if (currentCardData.currentHealth <= 0)
        {
            yield return _interactionDataUpdater.Handle_StateUpdate();
            yield return _interactionDataUpdater.Handle_HealthUpdate(0);

            Reset_ActionDatas();
            yield break;
        }

        int currentTurnCount = GameManager.instance.stageManager.endTurnCount;

        if (currentCardData.Has_State(InteractableState.Frozen, out int addedTurnCount) && addedTurnCount < currentTurnCount)
        {
            currentCardData.Remove_State(InteractableState.Frozen);
            yield return _interactionDataUpdater.Handle_StateUpdate();

            Reset_ActionDatas();
            yield break;
        }

        if (CustomData_Utility.AllConditions_ActivateAvailable(_placedTile, _currentTileConditions) == false)
        {
            Reset_ActionDatas();
            yield break;
        }

        yield return _preUpdateSkillBus.RunSequential_DelayBusEvents();
        yield return Run_TileTargeting_HealthModify();
        yield return _afterUpdateSkillBus.RunSequential_DelayBusEvents();

        Reset_ActionDatas();
        yield break;
    }
}