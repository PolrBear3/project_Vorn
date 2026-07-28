using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionData
{
    [SerializeField][Range(0, 10)] private int _mana;
    public int mana => _mana;

    [SerializeField][Range(0, 100)] private int _health;
    public int health => _health;

    [SerializeField][Range(0, 100)] private int _damage;
    public int damage => _damage;


    [Space(10)]
    [SerializeField][Range(0, 10)] private int _interactRange;
    public int interactRange => _interactRange;

    [SerializeField][Range(0, 10)] private int _targetSelectCount;
    public int targetSelectCount => _targetSelectCount;
}
