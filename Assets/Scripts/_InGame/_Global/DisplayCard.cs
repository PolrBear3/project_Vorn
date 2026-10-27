using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [Space(20)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _contentImage;

    [Space(10)]
    [SerializeField] private Image _IndicationIconImage;


    public Action<DisplayCard> OnHover;
    public Action<DisplayCard> OnSelect;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        _hoverDetector.OnPointerState += Hover;
    }

    public void UnSubscribe_All()
    {
        _hoverDetector.OnPointerState -= Hover;
    }


    // UI
    public void Load_CardComponents(Card_ScrObj cardToLoad)
    {
        bool toggle = cardToLoad != null;
        gameObject.SetActive(toggle);

        if (toggle == false) return;
        _contentImage.sprite = cardToLoad.contentSprite;
    }

    public void Load_IndicationIcon(Sprite indicationSprite)
    {
        bool toggle = indicationSprite != null;
        _IndicationIconImage.gameObject.SetActive(toggle);

        if (toggle == false) return;
        _IndicationIconImage.sprite = indicationSprite;
    }


    // Interaction
    public void Hover(bool hoverDetected)
    {
        OnHover?.Invoke(hoverDetected ? this : null);
    }

    public void Select()
    {
        OnSelect?.Invoke(this);
    }
}
