using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage/Battle Stage")]
public class BattleStage_ScrObj : Stage_ScrObj
{
    [Space(40)]
    [SerializeField] private Sprite[] _defaultTileSprites;
    [SerializeField] private Sprite[] _edgeTilesprites;

    [Space(20)]
    [SerializeField][Range(0, 50)] private int _rowTileCount;
    public int rowTileCount => _rowTileCount;

    [SerializeField][Range(0, 50)] private int _columnTileCount;
    public int columnTileCount => _columnTileCount;


    public Sprite Default_TileSprite()
    {
        return _defaultTileSprites[Random.Range(0, _defaultTileSprites.Length)];
    }
    public Sprite Edge_TileSprite()
    {
        return _edgeTilesprites[Random.Range(0, _edgeTilesprites.Length)];
    }

    [Space(20)]
    [SerializeField] private Enemy_SpawnData[] _enemySpawnDatas; // wave of enemies spawining in a single stage
    public Enemy_SpawnData[] enemySpawnDatas => _enemySpawnDatas;
}
