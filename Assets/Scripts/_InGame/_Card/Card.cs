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

    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


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