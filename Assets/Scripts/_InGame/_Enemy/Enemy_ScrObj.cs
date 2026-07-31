using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Enemy")]
public class Enemy_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private Vector2 _spawnOffset;
    public Vector2 spawnOffset => _spawnOffset;

    [Space(20)]
    [SerializeField] private InteractionData _interactionData;
    public InteractionData interactionData => _interactionData;
}