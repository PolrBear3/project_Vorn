using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Hero")]
public class Hero_ScrObj : CharacterScrObj
{
    [SerializeField][Range(0, 50)] private int _maxManaCount;
    public int maxManaCount => _maxManaCount;

    [Space(40)]
    [SerializeField] private Card_ScrObj _spawnCard;
    public Card_ScrObj spawnCard => _spawnCard;

    [SerializeField] private Sprite _cardPlatformSprite;
    public Sprite cardPlatformSprite => _cardPlatformSprite;

    // movement mana cost ?
}