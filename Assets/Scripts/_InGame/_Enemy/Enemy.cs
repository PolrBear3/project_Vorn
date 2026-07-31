using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    private EnemyData _data;
    public EnemyData data => _data;
    
    
    // MonoBehaviour
    private void Awake()
    {
        
    }
    
    private void OnDestroy()
    {
        
    }
    

    // Data
    private void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);
    }
}
