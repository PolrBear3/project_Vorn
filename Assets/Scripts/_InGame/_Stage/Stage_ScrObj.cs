using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/New Stage")]
public class Stage_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private Enemy_SpawnData[] _enemySpawnDatas; // wave of enemies spawining in a single stage
    public Enemy_SpawnData[] enemySpawnDatas => _enemySpawnDatas;

    
}