using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSkillTrigger
{
    Place,
    PreUpdate,
    AfterUpdate,
    PreTargeting,
    AfterTargeting,
    HealthUpdate,
    Death
}

public enum CardSkillTarget
{   CurrentTile, 
    TargetingTile,
    InteractRangeTile,
    InteractRangeTiles 
}

public abstract class CardSkill : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Card _card;
    public Card card => _card;

    [Space(10)]
    [SerializeField] private CardSkillTrigger _trigger;
    public CardSkillTrigger trigger => _trigger;

    [SerializeField] private CardSkillTarget _target;
    public CardSkillTarget target => _target;


    // Data
    public EventBus_Controller SkillTrigger_EventBus()
    {
        switch (_trigger)
        {
            case CardSkillTrigger.Place: return _card.placeUpdateActionBus;
            case CardSkillTrigger.PreUpdate: return _card.preUpdateSkillBus;
            case CardSkillTrigger.AfterUpdate: return _card.afterUpdateSkillBus;
            case CardSkillTrigger.PreTargeting: return _card.preTargetingSkillBus;
            case CardSkillTrigger.AfterTargeting: return _card.afterTargetingSkillBus;
            case CardSkillTrigger.HealthUpdate: return _card.healthUpdateActionBus;
            case CardSkillTrigger.Death: return _card.deathUpdateActionBus;
        }
        return null;
    }
}