using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, ISubscriptionHandler, ITileTargeting, IInteractable, IRunTimeAction
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
    [SerializeField] private ActivateCondition_ScrObj[] _targetingConditions;
    [SerializeField] private CardSkill_TriggerData[] _skillDatas;


    private CardData _data;
    public CardData data => _data;

    public Action OnSetData;


    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;

    private RunTimeAction_Data _runTimeAction = new();
    public RunTimeAction_Data runTimeAction => _runTimeAction;


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

    // IInteractable
    public InteractionData interactionData => _data.currentData;

    // IRunTimeAction
    public RunTimeAction_Data runTimeActionData => _runTimeAction;


    // MonoBehaviour
    private void OnDestroy()
    {
        UnSubscribe_All();
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {

    }

    public void UnSubscribe_All()
    {
        UnRegister_SkillDatas();

        _interactionDataUpdater.AfterDeathUpdate -= Remove_Data;
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
        _interactionDataUpdater.AfterDeathUpdate += Remove_Data;

        Register_SkillDatas();

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
        _runTimeAction.Toggle_RunAction(false);
    }

    private IEnumerator Run_TileTargeting_HealthModify()
    {
        if (CustomData_Utility.AllConditions_ActivateAvailable(_placedTile, _targetingConditions) == false) yield break;

        List<Tile> targetingTiles = new(_tileTargeting.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            Tile tile = targetingTiles[i];
            _runTimeAction.Update_TargetingTile(tile);

            yield return _preTargetingSkillBus.RunSequential_DelayBusEvents();

            IInteractable tileInteractable = tile.CurrentOccupant_Interactable();
            if (tileInteractable != null)
            {
                InteractionData targetData = tileInteractable.interactionData;
                if (targetData != null)
                {
                    // run health updating animation (animation is set relative to updateValue) ?
                    // while (_baseAnimator.CurrentState_Playing()) yield return null;

                    bool modifySuccessful = interactionData.Modify_OtherData_CurrentHealth(targetData);
                    _runTimeAction.CountUpdate_TargetingHealthModify(modifySuccessful);

                    yield return null;
                    while (targetData.dataUpdating) yield return null;
                }
            }

            yield return _afterTargetingSkillBus.RunSequential_DelayBusEvents();
        }
    }
    public IEnumerator Run_EndTurnActions()
    {
        if (_data.currentData.Remove_State(InteractableState.Frozen))
        {
            Reset_ActionDatas();
            yield break;
        }

        _runTimeAction.Toggle_RunAction(true);

        yield return _preUpdateSkillBus.RunSequential_DelayBusEvents();
        yield return Run_TileTargeting_HealthModify();
        yield return _afterUpdateSkillBus.RunSequential_DelayBusEvents();

        Reset_ActionDatas();
        yield break;
    }
}