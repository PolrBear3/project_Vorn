using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Enemy")]
public class Enemy_ScrObj : CharacterScrObj
{
    [Space(40)]
    [SerializeField][Range(0, 10)] private int _movementRange;
    public int movementRange => _movementRange;
}