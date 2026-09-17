using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IInteractable
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;

    [Space(10)]
    [SerializeField] private InteractionData_UpdateController _interactionDataUpdater;
    public InteractionData_UpdateController interactionDataUpdater => _interactionDataUpdater;

    [SerializeField] private ActionClock _actionClock;

    [Space(20)]
    [SerializeField] private EnemySkill_TriggerData[] _skillDatas;
    public EnemySkill_TriggerData[] skillDatas => _skillDatas;


    private EnemyData _data;
    public EnemyData data => _data;


    private EventBus_Controller _preMovementSkillBus = new();
    public EventBus_Controller preMovementSkillBus => _preMovementSkillBus;

    private EventBus_Controller _afterMovementSkillBus = new();
    public EventBus_Controller afterMovementSkillBus => _afterMovementSkillBus;

    private EventBus_Controller _preDamageSkillBus = new();
    public EventBus_Controller preDamageSkillBus => _preDamageSkillBus;

    private EventBus_Controller _afterDamageSkillBus = new();
    public EventBus_Controller afterDamageSkillBus => _afterDamageSkillBus;


    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;


    // IInteractable
    public InteractionData interactionData => _data.currentData;


    // MonoBehaviour
    private void OnDestroy()
    {
        UnRegister_SkillDatas();

        // from Set_Data
        _interactionDataUpdater.AfterDeathUpdate -= Remove_Data;
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);

        Register_SkillDatas();

        _interactionDataUpdater.Set_Data(_data.currentData);
        _interactionDataUpdater.AfterDeathUpdate += Remove_Data;

        _actionClock.Toggle(false);
    }
    private void Remove_Data()
    {
        GameManager.instance.enemyManager.spawnedEnemies.Remove(this);
        Destroy(gameObject);
    }


    // Movement
    private Tile Hero_TargetTile()
    {
        GameManager manager = GameManager.instance;

        Hero currentHero = manager.heroManager.currentHero;
        if (currentHero == null) return null;

        Tile targetTile = manager.tileManager.ClosestAvailable_SurroundingTile(_movement.currentTile, currentHero.movement.currentTile);
        if (targetTile == null) return null;

        return targetTile;
    }
    private Tile TauntCard_TargetTile()
    {
        Tile currentTile = _movement.currentTile;

        GameManager manager = GameManager.instance;

        List<Card> closestCards = manager.cardManager.TileClosest_PlacedCards(currentTile);
        if (closestCards.Count <= 0) return null;

        for (int i = 0; i < closestCards.Count; i++)
        {
            Card card = closestCards[i];

            if (card.data.currentData.states.Contains(InteractableState.Taunt) == false) continue;
            return manager.tileManager.ClosestAvailable_SurroundingTile(currentTile, card.placedTile);
        }
        return null;
    }

    private void Moveto_TargetTile()
    {
        Tile currentTile = _movement.currentTile;
        Tile destinationTile = TauntCard_TargetTile() ?? Hero_TargetTile();

        if (destinationTile == null || currentTile == destinationTile) return;

        List<Tile> routeTiles = GameManager.instance.tileManager.PathFind_RouteTiles(currentTile, destinationTile);
        if (routeTiles.Count <= 0) return;

        Tile routeTile = routeTiles[0];

        _movement.Direction_FlipUpdate(routeTile);
        _movement.Moveto_Tile(routeTiles[0], _data.enemyScrObj.spawnOffset); // set routTile index value relative to movement range ?
    }


    // Damage
    private InteractionData DamageTarget_InteractionData()
    {
        GameManager manager = GameManager.instance;
        CardManager cardManager = manager.cardManager;

        Tile currentTile = _movement.currentTile;
        Vector2 currentTilePos = currentTile.data.position;

        int interactRange = _data.currentData.interactRange;

        // taunt card
        Card tauntCard = cardManager.TileClosest_PlacedCard(currentTile, cardManager.TileClosest_PlacedCards(currentTile, InteractableState.Taunt));
        bool tauntCardDamageable = tauntCard != null && Utility.Chebyshev_Distance(currentTilePos, tauntCard.placedTile.data.position) <= interactRange;

        if (tauntCardDamageable) return tauntCard.interactionData;
        if (tauntCard != null) return null; // restrict damaging if taunt cards are not in range

        // hero
        Hero currentHero = manager.heroManager.currentHero;
        if (currentHero != null && Utility.Chebyshev_Distance(currentTile.data.position, currentHero.movement.currentTile.data.position) <= interactRange)
        {
            return currentHero.interactionData;
        }

        // interact range card
        Card damageCard = cardManager.TileClosest_PlacedCard(currentTile);
        if (damageCard == null) return null;

        int distanceToCard = Utility.Chebyshev_Distance(currentTile.data.position, damageCard.placedTile.data.position);
        if (distanceToCard > interactRange) return null;

        return damageCard.interactionData;
    }
    private InteractionData Damage_RangedInteractable()
    {
        InteractionData damageTargetData = DamageTarget_InteractionData();
        if (damageTargetData == null) return null;

        int damageUpdateValue = damageTargetData.currentHealth + _data.currentData.healthModifyValue;
        damageTargetData.Update_CurrentHealth(damageUpdateValue);

        return damageTargetData;
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
    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;
        _actionClock.Toggle(_actionsRunning);

        yield return _preMovementSkillBus.RunSequential_DelayBusEvents();

        int movementRange = _data.movementRange; // movement
        for (int i = 0; i < movementRange; i++)
        {
            Moveto_TargetTile();
            while (_movement.movementCoroutine != null) yield return null;
        }
        yield return _afterMovementSkillBus.RunSequential_DelayBusEvents();

        yield return _preDamageSkillBus.RunSequential_DelayBusEvents();
        InteractionData damageData = Damage_RangedInteractable(); // damage interactable

        if (damageData != null)
        {
            yield return null;
            while (damageData.dataUpdating) yield return null;
        }
        yield return _afterDamageSkillBus.RunSequential_DelayBusEvents();

        _actionsRunning = false;
        _actionClock.Toggle(_actionsRunning);

        yield break;
    }
}