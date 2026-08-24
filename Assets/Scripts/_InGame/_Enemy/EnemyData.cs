using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    private Enemy_ScrObj _enemyScrObj;
    public Enemy_ScrObj enemyScrObj => _enemyScrObj;

    private InteractionData _currentData;
    public InteractionData currentData => _currentData;

    private int _movementRange;
    public int movementRange => _movementRange;

    // destroyed card list ?


    // New
    public EnemyData(Enemy_ScrObj setEnemy)
    {
        _enemyScrObj = setEnemy;
        _currentData = new(setEnemy.interactionData);
        _movementRange = setEnemy.movementRange;
    }
}
