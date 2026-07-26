using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCard : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    public RectTransform rectTransform => _rectTransform;

    [Space(20)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _contentImage;

    private CardData _data;
    public CardData data => _data;


    // Data
    public void Load(CardData loadCardData)
    {
        if (loadCardData == null) return;
        if (loadCardData.cardScrObj == null) return;

        _data = loadCardData;
        _contentImage.sprite = _data.cardScrObj.contentSprite;
    }
    public void Load(Card_ScrObj setCard)
    {
        Load(new CardData(setCard));
    }
}