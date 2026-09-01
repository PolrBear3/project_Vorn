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

    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;


    // IInteractable
    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // ITileTargeting
    public Tile pivotTile => _movement.currentTile;
    public TileTargeting_Data targetingData => _tileTargeting;
    public int targetingCount => _data.currentData.targetSelectCount;

    public bool Targeting_Available(Tile targetingTile)
    {
        List<Tile> routeTiles = GameManager.instance.tileManager.PathFind_RouteTiles(_movement.currentTile, targetingTile);

        return targetingTile.currentOccupant == null && routeTiles.Contains(targetingTile);
    }


    // Data
    public void Set_Data(Hero_ScrObj setHero)
    {
        _data = new(setHero);
        _healthController.Set_Data(_data.currentData);
    }


    // End Turn
    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;

        // movement
        TileManager tileManager = GameManager.instance.tileManager;
        List<Tile> targetingDestinationTiles = _tileTargeting.targetingTiles;

        if (targetingDestinationTiles.Count > 0)
        {
            Tile destinationTile = targetingDestinationTiles[0];
            List<Tile> routeTiles = tileManager.PathFind_RouteTiles(_movement.currentTile, destinationTile);

            for (int i = 0; i < routeTiles.Count; i++)
            {
                _movement.Direction_FlipUpdate(routeTiles[i]);
                _movement.Moveto_Tile(routeTiles[i], _data.heroScrObj.spawnOffset);

                while (_movement.movementCoroutine != null) yield return null;
            }
        }

        _actionsRunning = false;
        yield break;
    }
}
