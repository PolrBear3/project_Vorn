using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage")]
public class Stage_ScrObj : ScriptableObject
{
    [Space(10)]
    // wave of enemies spawining in a single stage
    [SerializeField] private Enemy_SpawnData[] _enemySpawnDatas;
    public Enemy_SpawnData[] enemySpawnDatas => _enemySpawnDatas;
}