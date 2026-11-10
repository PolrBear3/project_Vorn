using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, ISubscriptionHandler, IInteractable, IRunTimeAction
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;

    [Space(10)]
    [SerializeField] private InteractionData_UpdateController _interactionDataUpdater;
    public InteractionData_UpdateController interactionDataUpdater => _interactionDataUpdater;

    [Space(10)]
    [SerializeField] private InteractionBar _healthBar;
    public InteractionBar healthBar => _healthBar;

    [SerializeField] private HealthModify_Indicator _modifyIndicator;
    public HealthModify_Indicator modifyIndicator => _modifyIndicator;

    [Space(20)]
    [SerializeField] private EnemySkill_TriggerData[] _skillDatas;
    public EnemySkill_TriggerData[] skillDatas => _skillDatas;


    private EnemyData _data;
    public EnemyData data => _data;

    private Tile _damagingTile;
    public Tile damagingTile => _damagingTile;

    private RunTimeAction_Data _runTimeAction = new();
    public RunTimeAction_Data runTimeActionData => _runTimeAction;


    private EventBus_Controller _preMovementSkillBus = new();
    public EventBus_Controller preMovementSkillBus => _preMovementSkillBus;

    private EventBus_Controller _afterMovementSkillBus = new();
    public EventBus_Controller afterMovementSkillBus => _afterMovementSkillBus;

    private EventBus_Controller _preDamageSkillBus = new();
    public EventBus_Controller preDamageSkillBus => _preDamageSkillBus;

    private EventBus_Controller _afterDamageSkillBus = new();
    public EventBus_Controller afterDamageSkillBus => _afterDamageSkillBus;


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
        InteractionData currentData = _data.currentData;

        currentData.OnHealthUpdate += _healthBar.Update_Fill;
        currentData.OnStateUpdate += _interactionDataUpdater.Handle_StateUpdate;

        _interactionDataUpdater.OnPreDeathActionDataUpdate += Remove_Data;
    }

    public void UnSubscribe_All()
    {
        UnRegister_SkillDatas();

        InteractionData currentData = _data.currentData;

        currentData.OnHealthUpdate -= _healthBar.Update_Fill;
        currentData.OnStateUpdate -= _interactionDataUpdater.Handle_StateUpdate;

        _interactionDataUpdater.OnPreDeathActionDataUpdate -= Remove_Data;
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);
        _interactionDataUpdater.Set_Data(_data.currentData);

        Subscribe_All();
        Register_SkillDatas();

        InteractionData currentData = _data.currentData;

        _healthBar.Update_Fill(currentData.maxHealth, currentData.currentHealth);
        _healthBar.Toggle_ValueBox(false);

        _modifyIndicator.Update_ModifyValue(currentData.healthModifyValue);
        _modifyIndicator.Toggle_ValueBox(true);
        _modifyIndicator.gameObject.SetActive(false);
    }
    private void Remove_Data()
    {
        GameManager.instance.enemyManager.spawnedEnemies.Remove(this);
        _movement.currentTile.Update_CurrentOccupant(null);
    }


    // Movement
    private Tile TauntCard_TargetTile()
    {
        GameManager manager = GameManager.instance;

        Tile currentTile = _movement.currentTile;
        List<Card> closestCards = manager.cardManager.TileClosest_PlacedCards(currentTile);

        if (closestCards.Count <= 0) return null;

        for (int i = 0; i < closestCards.Count; i++)
        {
            Card card = closestCards[i];

            if (card.data.currentData.states.Contains(InteractableState.Taunt) == false) continue;
            return card.placedTile;
        }
        return null;
    }
    private Tile Hero_TargetTile()
    {
        GameManager manager = GameManager.instance;

        Hero currentHero = manager.heroManager.spawnedHero;
        if (currentHero == null) return null;

        return currentHero.movement.currentTile;
    }

    public Tile TargetTile()
    {
        return TauntCard_TargetTile() ?? Hero_TargetTile();
    }
    private void Moveto_TargetTile()
    {
        TileManager tileManager = GameManager.instance.tileManager;

        Tile currentTile = _movement.currentTile;
        Tile destinationTile = tileManager.ClosestAvailable_SurroundingTile(currentTile, TargetTile());

        if (destinationTile == null || currentTile == destinationTile) return;

        List<Tile> routeTiles = tileManager.PathFind_RouteTiles(currentTile, destinationTile);
        if (routeTiles.Count <= 0) return;

        Tile routeTile = routeTiles[0];

        _movement.Direction_FlipUpdate(routeTile);
        _movement.Moveto_Tile(routeTiles[0], _data.enemyScrObj.spawnOffset); // set routTile index value relative to movement range ?
    }


    // Damage
    private InteractionData_UpdateController Damageable_InteractionData(out Tile damageTargetTile)
    {
        damageTargetTile = null;

        GameManager manager = GameManager.instance;
        CardManager cardManager = manager.cardManager;

        Tile currentTile = _movement.currentTile;
        Vector2 currentTilePos = currentTile.data.position;

        int interactRange = _data.currentData.interactRange;

        // taunt card
        Card tauntCard = cardManager.TileClosest_PlacedCard(currentTile, cardManager.TileClosest_PlacedCards(currentTile, InteractableState.Taunt));
        bool tauntCardDamageable = tauntCard != null && Utility.Chebyshev_Distance(currentTilePos, tauntCard.placedTile.data.position) <= interactRange;

        if (tauntCardDamageable)
        {
            damageTargetTile = tauntCard.placedTile;
            return tauntCard.interactionDataUpdater;
        }
        if (tauntCard != null) return null; // restrict damaging if taunt cards are not in range

        // hero
        Hero currentHero = manager.heroManager.spawnedHero;
        if (currentHero != null && Utility.Chebyshev_Distance(currentTilePos, currentHero.movement.currentTile.data.position) <= interactRange)
        {
            damageTargetTile = currentHero.movement.currentTile;
            return currentHero.interactionDataUpdater;
        }

        // interact range card
        Card damageCard = cardManager.TileClosest_PlacedCard(currentTile);
        if (damageCard == null) return null;

        int distanceToCard = Utility.Chebyshev_Distance(currentTilePos, damageCard.placedTile.data.position);
        if (distanceToCard > interactRange) return null;

        damageTargetTile = damageCard.placedTile;
        return damageCard.interactionDataUpdater;
    }
    private int Damage_InteractionData(InteractionData damageData)
    {
        if (damageData == null) return 0;

        int damageValue = _data.currentData.Modify_OtherData_CurrentHealth(damageData);
        _runTimeAction.CountUpdate_TargetingHealthModify(damageValue != 0);

        return damageValue;
    }


    // Skill
    private EventBus_Controller SkillTrigger_EventBus(EnemySkillTrigger trigger)
    {
        switch (trigger)
        {
            case EnemySkillTrigger.PreMovement: return _preMovementSkillBus;
            case EnemySkillTrigger.AfterMovement: return _afterMovementSkillBus;
            case EnemySkillTrigger.PreDamaging: return _preDamageSkillBus;
            case EnemySkillTrigger.AfterDamaging: return _afterDamageSkillBus;
            case EnemySkillTrigger.Death: return _interactionDataUpdater.deathUpdateActionBus;
        }
        return null;
    }

    private void Register_SkillDatas()
    {
        for (int i = 0; i < _skillDatas.Length; i++)
        {
            EnemySkill_TriggerData data = _skillDatas[i];

            EventBus_Controller triggerBus = SkillTrigger_EventBus(data.trigger);
            if (triggerBus == null) continue;

            EnemySkill[] triggerSkills = data.enemySkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                EnemySkill skill = triggerSkills[j];
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
            EnemySkill_TriggerData data = _skillDatas[i];

            EventBus_Controller triggerBus = SkillTrigger_EventBus(data.trigger);
            if (triggerBus == null) return;

            EnemySkill[] triggerSkills = data.enemySkills;
            for (int j = 0; j < triggerSkills.Length; j++)
            {
                EnemySkill skill = triggerSkills[j];
                if (skill == null) continue;

                triggerBus.UnRegister(skill.Run_SkillTrigger);
            }
        }
    }


    // End Turn
    private void Reset_ActionDatas()
    {
        _healthBar.Toggle_ValueBox(false);
        _modifyIndicator.gameObject.SetActive(false);

        _runTimeAction.Toggle_RunAction(false);
    }

    public IEnumerator Run_EndTurnActions()
    {
        _runTimeAction.Toggle_RunAction(true);

        _healthBar.Toggle_ValueBox(true);
        _modifyIndicator.gameObject.SetActive(true);

        // check death before action
        if (_data.currentData.currentHealth <= 0)
        {
            yield return _interactionDataUpdater.Handle_HealthUpdate(0);

            Reset_ActionDatas();
            yield break;
        }

        // movement
        yield return _preMovementSkillBus.RunSequential_DelayBusEvents();

        int movementRange = _data.movementRange;
        for (int i = 0; i < movementRange; i++)
        {
            Moveto_TargetTile();
            while (_movement.movementCoroutine != null) yield return null;
        }

        InteractionData_UpdateController damageDataController = Damageable_InteractionData(out Tile damageTargetTile); // damaging interactable
        _damagingTile = damageTargetTile;

        yield return _afterMovementSkillBus.RunSequential_DelayBusEvents();

        if (damageDataController != null)
        {
            yield return _preDamageSkillBus.RunSequential_DelayBusEvents();

            int damageValue = Damage_InteractionData(damageDataController.targetData);
            yield return damageDataController.Handle_HealthUpdate(damageValue);

            yield return _afterDamageSkillBus.RunSequential_DelayBusEvents();
        }

        _damagingTile = null;

        _healthBar.Toggle_ValueBox(false);
        _modifyIndicator.gameObject.SetActive(false);

        _runTimeAction.Toggle_RunAction(false);
        yield break;
    }
}