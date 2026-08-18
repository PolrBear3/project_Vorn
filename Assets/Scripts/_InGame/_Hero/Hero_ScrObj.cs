using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Hero")]
public class Hero_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private GameObject _spawnPrefab;
    public GameObject spawnPrefab => _spawnPrefab;

    [SerializeField] private Card_ScrObj _spawnCard;
    public Card_ScrObj spawnCard => _spawnCard;
}