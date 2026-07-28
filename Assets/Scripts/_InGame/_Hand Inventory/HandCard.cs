using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCard : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [Space(20)]
    [SerializeField] private RectTransform _rectTransform;
    public RectTransform rectTransform => _rectTransform;

    [Space(10)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _contentImage;


    private CardData _data;
    public CardData data => _data;


    // MonoBehaviour
    private void Awake()
    {
        _hoverDetector.OnPointerState += Update_OnHover;
    }
    
    private void OnDestroy()
    {
        _hoverDetector.OnPointerState -= Update_OnHover;
    }


    // Data
    public void Load(CardData loadCardData)
    {
        if (loadCardData == null) return;
        if (loadCardData.cardScrObj == null) return;

        _data = loadCardData;

        _rectTransform.localScale = new(1, 1, 1);
        _contentImage.sprite = _data.cardScrObj.contentSprite;
    }
    public void Load(Card_ScrObj setCard)
    {
        Load(new CardData(setCard));
    }


    // Hover
    private void Update_OnHover(bool isHovering)
    {
        GameManager.instance.handInventory.Update_HoveringCard(isHovering ? this : null);
    }
}