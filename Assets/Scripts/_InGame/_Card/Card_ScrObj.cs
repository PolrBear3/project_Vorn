using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Card")]
public class Card_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private Sprite _contentSprite;
    public Sprite contentSprite => _contentSprite;

    [Space(20)]
    [SerializeField] private InteractionData _interactionData;
    public InteractionData interactionData => _interactionData;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _mana;
    public int mana => _mana;
}