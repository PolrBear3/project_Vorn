using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private GameObject _spawnPrefab;
    public GameObject spawnPrefab => _spawnPrefab;

    [SerializeField] private Vector2 _spawnOffset;
    public Vector2 spawnOffset => _spawnOffset;

    [Space(10)]
    [SerializeField] private InteractionData _interactionData;
    public InteractionData interactionData => _interactionData;
}
