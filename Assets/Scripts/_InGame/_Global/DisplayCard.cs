using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [Space(20)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _contentImage;

    [Space(10)]
    [SerializeField] private Image _IndicationIconImage;


    public Action<DisplayCard> OnSelect;


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


    // Hover


    // Select
    public void Select()
    {
        OnSelect?.Invoke(this);
    }
}
