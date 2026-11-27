using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, ISubscriptionHandler, IInteractable
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
    [Tooltip("Condition for starting the action.")]
    [SerializeField] private ActivateCondition_ScrObj[] _currentTileConditions;

    [Tooltip("Condition for target tile to move to when there are no taunt cards placed.")]
    [SerializeField] private ActivateCondition_ScrObj[] _targetTileConditions;

    [Space(20)]
    [SerializeField] private EnemySkill_TriggerData[] _skillDatas;
    public EnemySkill_TriggerData[] skillDatas => _skillDatas;


    private EnemyData _data;
    public EnemyData data => _data;

    private Tile _damagingTile;
    public Tile damagingTile => _damagingTile;


    private EventBus_Controller _spawnSkillBus = new();
    public EventBus_Controller spawnSkillBus => _spawnSkillBus;

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
        _data.currentData.OnHealthUpdate += _healthBar.Update_Fill;
        _spawnSkillBus.Register(0, _interactionDataUpdater.Handle_StateUpdate);

        _interactionDataUpdater.OnPreDeathActionDataUpdate += Remove_Data;

        GameManager.instance.stageManager.endTurnEventBus.Register(0, _data.currentData.Track_TurnCount);
    }

    public void UnSubscribe_All()
    {
        UnRegister_SkillDatas();

        _data.currentData.OnHealthUpdate += _healthBar.Update_Fill;
        _spawnSkillBus.UnRegister(_interactionDataUpdater.Handle_StateUpdate);

        _interactionDataUpdater.OnPreDeathActionDataUpdate -= Remove_Data;

        GameManager.instance.stageManager.endTurnEventBus.UnRegister(_data.currentData.Track_TurnCount);
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

            if (card.data.currentData.Has_State(InteractableState.Taunt) == false) continue;
            return card.placedTile;
        }
        return null;
    }
    private Tile Condition_TargetTile()
    {
        if (_targetTileConditions.Length <= 0) return null;

        List<Tile> targetTiles = GameManager.instance.tileManager.CloseSorted_Tiles(_movement.currentTile);

        for (int i = 0; i < targetTiles.Count; i++)
        {
            Tile targetTile = targetTiles[i];

            if (CustomData_Utility.Conditions_ActivateAvailable(targetTile, _targetTileConditions) == false) continue;
            return targetTile;
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
        return TauntCard_TargetTile() ?? Condition_TargetTile() ?? Hero_TargetTile();
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
    private IInteractable DamageTarget_Interactable(out Tile damageTargetTile)
    {
        damageTargetTile = null;

        // condition target tile check
        if (Condition_TargetTile() != null) return null;

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
            return tauntCard;
        }
        if (tauntCard != null) return null; // restrict damaging if taunt cards are not in range

        // hero
        Hero currentHero = manager.heroManager.spawnedHero;
        if (currentHero != null && Utility.Chebyshev_Distance(currentTilePos, currentHero.movement.currentTile.data.position) <= interactRange)
        {
            damageTargetTile = currentHero.movement.currentTile;
            return currentHero;
        }

        // interact range card
        Card damageCard = cardManager.TileClosest_PlacedCard(currentTile);
        if (damageCard == null) return null;

        int distanceToCard = Utility.Chebyshev_Distance(currentTilePos, damageCard.placedTile.data.position);
        if (distanceToCard > interactRange) return null;

        damageTargetTile = damageCard.placedTile;
        return damageCard;
    }
    private int Damage_Interactable(IInteractable targetInteractable)
    {
        if (targetInteractable == null) return 0;

        InteractionData currentEnemyData = _data.currentData;
        int damageValue = currentEnemyData.healthModifyValue;

        InteractionData targetData = targetInteractable.interactionDataUpdater.targetData;

        targetData.Update_CurrentHealth(targetData.currentHealth + damageValue, out int modifiedValue);
        currentEnemyData.Track_HealthModifyHistory(modifiedValue);

        return modifiedValue;
    }


    // Skill
    private EventBus_Controller SkillTrigger_EventBus(EnemySkillTrigger trigger)
    {
        switch (trigger)
        {
            case EnemySkillTrigger.Spawn: return _preMovementSkillBus;
            case EnemySkillTrigger.PreMovement: return _preMovementSkillBus;
            case EnemySkillTrigger.AfterMovement: return _afterMovementSkillBus;
            case EnemySkillTrigger.PreDamaging: return _preDamageSkillBus;
            case EnemySkillTrigger.AfterDamaging: return _afterDamageSkillBus;
            case EnemySkillTrigger.HealthUpdate: return _interactionDataUpdater.healthUpdateActionBus;
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

                triggerBus.Register(j, skill.Trigger_Skill);
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

                triggerBus.UnRegister(skill.Trigger_Skill);
            }
        }
    }


    // End Turn
    private void Reset_ActionDatas()
    {
        _damagingTile = null;

        _healthBar.Toggle_ValueBox(false);
        _modifyIndicator.gameObject.SetActive(false);
    }
    public IEnumerator Run_EndTurnActions()
    {
        _healthBar.Toggle_ValueBox(true);
        _modifyIndicator.gameObject.SetActive(true);

        InteractionData enemyInteractionData = _data.currentData;


        // check death before action
        if (enemyInteractionData.currentHealth <= 0)
        {
            yield return _interactionDataUpdater.Handle_StateUpdate();
            yield return _interactionDataUpdater.Handle_HealthUpdate(0);

            Reset_ActionDatas();
            yield break;
        }


        // frozen state update
        int currentTurnCount = GameManager.instance.stageManager.endTurnCount;

        if (enemyInteractionData.Has_State(InteractableState.Frozen, out int addedTurnCount))
        {
            if (currentTurnCount <= 1 || addedTurnCount >= currentTurnCount)
            {
                Reset_ActionDatas();
                yield break;
            }

            enemyInteractionData.Remove_State(InteractableState.Frozen);
            yield return _interactionDataUpdater.Handle_StateUpdate();
        }


        // action start condition check
        if (CustomData_Utility.Conditions_ActivateAvailable(_movement.currentTile, _currentTileConditions) == false)
        {
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

        IInteractable damageTarget = DamageTarget_Interactable(out Tile damageTargetTile); // damaging interactable
        _damagingTile = damageTargetTile;

        yield return _afterMovementSkillBus.RunSequential_DelayBusEvents();

        if (damageTarget != null)
        {
            yield return _preDamageSkillBus.RunSequential_DelayBusEvents();
            int damageValue = Damage_Interactable(damageTarget);

            InteractionData_UpdateController damageTargetController = damageTarget.interactionDataUpdater;

            yield return damageTargetController.Handle_StateUpdate();  // for shield remove animation
            yield return damageTargetController.Handle_HealthUpdate(damageValue);

            yield return _afterDamageSkillBus.RunSequential_DelayBusEvents();
        }


        Reset_ActionDatas();
        yield break;
    }
}
