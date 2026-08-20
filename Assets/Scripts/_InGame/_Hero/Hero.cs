using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour, IInteractable
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;


    private HeroData _data;
    public HeroData data => _data;


    // IInteractable
    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // Data
    public void Set_Data(Hero_ScrObj setHero)
    {
        _data = new(setHero);
    }
}
