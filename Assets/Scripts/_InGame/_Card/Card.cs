using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    private CardData _data;
    public CardData data => _data;


    // Data
    public void Load(Card_ScrObj setCard)
    {
        if (setCard == null) return;
        _data = new(setCard);

        _contentSpriteRenderer.sprite = setCard.contentSprite;
    }
}
