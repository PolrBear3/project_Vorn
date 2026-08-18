using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAction : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Enemy _enemy;
    public Enemy enemy => _enemy;
}
