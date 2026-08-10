using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _baseSpriteRenderer;
    [SerializeField] private SpriteRenderer _contentSpriteRenderer;

    [Space(20)]
    [SerializeField] private TileTargeting_Controller _tileTargeting;
    public TileTargeting_Controller tileTargeting => _tileTargeting;


    private CardData _data;
    public CardData data => _data;

    private Tile _placedTile;
    public Tile placedTile => _placedTile;


    // Data
    public void Load(CardData loadData, Tile placeTile)
    {
        if (loadData == null) return;

        Card_ScrObj loadCard = loadData.cardScrObj;
        if (loadCard == null) return;

        _data = loadData;
        _placedTile = placeTile;

        _contentSpriteRenderer.sprite = loadCard.contentSprite;
    }
    public void Load(Card_ScrObj loadCard, Tile placeTile)
    {
        Load(new CardData(loadCard), placeTile);
    }
}