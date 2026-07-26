using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionData
{
    [SerializeField][Range(0, 100)] private int _health;
    public int health => _health;

    [SerializeField][Range(0, 100)] private int _damage;
    public int damage => _damage;

    [Space(10)]
    [SerializeField] private List<Vector2> _interactPositions = new();
    public List<Vector2> interactPositions => _interactPositions;
}
