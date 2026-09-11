using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Card")]
public class Card_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private string _cardName;
    public string cardName => _cardName;

    [SerializeField][TextArea(3, 10)] private string _cardDescription;
    public string cardDescription => _cardDescription;

    [Space(20)]
    [SerializeField] private Sprite _contentSprite;
    public Sprite contentSprite => _contentSprite;

    [SerializeField] private GameObject _placePrefab;
    public GameObject placePrefab => _placePrefab;

    [Space(20)]
    [SerializeField] private InteractionData _interactionData;
    public InteractionData interactionData => _interactionData;

    [SerializeField][Range(0, 50)] private int _manaCost;
    public int manaCost => _manaCost;
}