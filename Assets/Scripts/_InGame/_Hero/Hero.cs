using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour, IInteractable, ITileTargeting
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;

    [Space(10)]
    [SerializeField] private InteractableHealth_Controller _healthController;
    public InteractableHealth_Controller healthController => _healthController;


    private HeroData _data;
    public HeroData data => _data;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;


    // IInteractable
    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // ITileTargeting
    public Tile pivotTile => _movement.currentTile;
    public TileTargeting_Data targetingData => _tileTargeting;

    public int targetingRange => _data.currentData.interactRange;
    public int targetingCount => _data.currentData.targetSelectCount;


    // Data
    public void Set_Data(Hero_ScrObj setHero)
    {
        _data = new(setHero);
        _healthController.Set_Data(_data.currentData);
    }
}
