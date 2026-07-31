using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData
{
    private Enemy_ScrObj _enemyScrObj;
    public Enemy_ScrObj enemyScrObj => _enemyScrObj;

    private InteractionData _currentData;
    public InteractionData currentData => _currentData;


    // New
    public EnemyData(Enemy_ScrObj setEnemy)
    {
        _enemyScrObj = setEnemy; 
        _currentData = setEnemy.interactionData;
    }
}
