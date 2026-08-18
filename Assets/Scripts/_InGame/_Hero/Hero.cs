using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;
}
