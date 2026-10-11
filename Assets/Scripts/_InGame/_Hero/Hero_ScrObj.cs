using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Hero")]
public class Hero_ScrObj : CharacterScrObj
{
    [SerializeField][Range(0, 50)] private int _maxManaCount;
    public int maxManaCount => _maxManaCount;

    [Space(60)]
    [SerializeField] private Sprite _cardPlatformSprite;
    public Sprite cardPlatformSprite => _cardPlatformSprite;

    [Space(20)]
    [SerializeField] private Card_ScrObj _spawnCard;
    public Card_ScrObj spawnCard => _spawnCard;

    [SerializeField] private Card_ScrObj[] _proprietaryCards;
    public Card_ScrObj[] proprietaryCards => _proprietaryCards;

    [Space(20)]
    [SerializeField][Range(0, 20)] private int _movementManaCost;
    public int movementManaCost => _movementManaCost;
}