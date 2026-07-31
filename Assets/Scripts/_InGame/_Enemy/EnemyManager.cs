using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _enemyPrefab;


    private List<Enemy> _spawnedEnemies = new();
    public List<Enemy> spawnedEnemies => _spawnedEnemies;


    // MonoBehaviour
    private void Awake()
    {
        
    }
    
    private void OnDestroy()
    {
        
    }
    

    // Spawn
    private void Spawn()
    {
        
    }
}