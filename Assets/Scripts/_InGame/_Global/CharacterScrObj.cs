using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private string _characterName;
    public string characterName => _characterName;

    [SerializeField][TextArea(3, 10)] private string _characterDescription;
    public string characterDescription => _characterDescription;

    [Space(10)]
    [SerializeField] private Sprite _toolTipBaseSprite;
    public Sprite toolTipBaseSprite => _toolTipBaseSprite;

    [Space(20)]
    [SerializeField] private GameObject _spawnPrefab;
    public GameObject spawnPrefab => _spawnPrefab;

    [SerializeField] private Vector2 _spawnOffset;
    public Vector2 spawnOffset => _spawnOffset;

    [Space(20)]
    [SerializeField] private InteractionData _interactionData;
    public InteractionData interactionData => _interactionData;
}
