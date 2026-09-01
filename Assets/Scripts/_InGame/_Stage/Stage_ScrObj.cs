using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage")]
public class Stage_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private Sprite[] _defaultTileSprites;
    [SerializeField] private Sprite[] _edgeTilesprites;

    [Space(20)]
    [SerializeField] private Enemy_SpawnData[] _enemySpawnDatas; // wave of enemies spawining in a single stage
    public Enemy_SpawnData[] enemySpawnDatas => _enemySpawnDatas;


    public Sprite Default_TileSprite()
    {
        return _defaultTileSprites[Random.Range(0, _defaultTileSprites.Length)];
    }
    public Sprite Edge_TileSprite()
    {
        return _edgeTilesprites[Random.Range(0, _edgeTilesprites.Length)];
    }
}