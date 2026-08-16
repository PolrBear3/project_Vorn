using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardSkill : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Card _card;
    public Card card => _card;
}