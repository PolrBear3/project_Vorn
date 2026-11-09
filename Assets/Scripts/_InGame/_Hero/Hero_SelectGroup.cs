using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero_SelectGroup : HoverUI, ISubscriptionHandler
{
    [Space(40)]
    [SerializeField] private EventSystems_Controller _heroHoverDetector;
    [SerializeField] private EventSystems_Controller _spawnCardHoverDetector;

    [Space(20)]
    [SerializeField] private Image _layoutPlatform;

    [Space(10)]
    [SerializeField] private Image _heroIcon;
    [SerializeField] private Image _spawnCardContent;

    [Space(20)]
    [SerializeField] private Sprite[] _coinSprites;
    [SerializeField] private Image[] _coinImages;


    public Action<Hero_SelectGroup> OnHeroHover;
    public Action<Hero_SelectGroup> OnSpawnCardHover;


    // MonoBehaviour
    public override void Awake()
    {
        base.Awake();


        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();


        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // HoverUI
    public override void OnHover_Update(bool _)
    {

    }

    public override void OnSelect_Update()
    {
        GameManager.instance.heroManager.heroSelectManager.Select_Hero(this);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        _heroHoverDetector.OnPointerState += Hover_Hero;
        _spawnCardHoverDetector.OnPointerState += Hover_SpawnCard;
    }

    public void UnSubscribe_All()
    {
        _heroHoverDetector.OnPointerState -= Hover_Hero;
        _spawnCardHoverDetector.OnPointerState -= Hover_SpawnCard;
    }


    // UI
    public void Update_SelectHero(Hero_ScrObj updateHero)
    {
        _layoutPlatform.sprite = updateHero.cardPlatformSprite;

        _heroIcon.sprite = updateHero.toolTipBaseSprite;
        _spawnCardContent.sprite = updateHero.spawnCard.contentSprite;
    }

    public void Update_Coins(int updateAmount)
    {
        int updateCount = Mathf.Clamp(updateAmount, 0, _coinImages.Length);

        for (int i = 0; i < _coinImages.Length; i++)
        {
            _coinImages[i].sprite = updateCount <= 0 ? _coinSprites[0] : _coinSprites[1];
            updateCount--;
        }
    }


    // Interaction
    public void Hover_Hero(bool hoverDetected)
    {
        OnHeroHover?.Invoke(hoverDetected ? this : null);
    }
    public void Hover_SpawnCard(bool hoverDetected)
    {
        OnSpawnCardHover?.Invoke(hoverDetected ? this : null);
    }
}