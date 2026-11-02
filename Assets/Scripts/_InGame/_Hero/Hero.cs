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
    [SerializeField] private InteractionData_UpdateController _healthController;
    public InteractionData_UpdateController dataUpdateController => _healthController;

    [Space(20)]
    [SerializeField] private InteractionBar _healthBar;
    public InteractionBar healthBar => _healthBar;

    [SerializeField] private ActionClock _actionClock;


    private HeroData _data;
    public HeroData data => _data;

    private TileTargeting_Data _tileTargeting = new();
    public TileTargeting_Data tileTargeting => _tileTargeting;

    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;


    // ITileTargeting
    public Tile pivotTile => _movement.currentTile;
    public TileTargeting_Data targetingData => _tileTargeting;
    public int targetingCount => _data.currentData.targetSelectCount;

    public bool Targeting_Available(Tile targetingTile)
    {
        List<Tile> routeTiles = GameManager.instance.tileManager.PathFind_RouteTiles(_movement.currentTile, targetingTile);

        if (targetingTile.currentOccupant != null) return false;
        return routeTiles.Contains(targetingTile) && _data.currentManaCount >= routeTiles.Count;
    }


    // Data
    public void Set_Data(Hero_ScrObj setHero)
    {
        _data = new(setHero);

        InteractionData currentData = _data.currentData;
        _healthController.Set_Data(currentData);

        _healthBar.Update_Fill(currentData.maxHealth, currentData.currentHealth);
        _healthBar.Toggle_ValueBox(false);

        _actionClock.Toggle(false);
    }


    // End Turn
    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;

        _healthBar.Toggle_ValueBox(true);
        _actionClock.Toggle(true);

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

        _healthBar.Toggle_ValueBox(false);
        _actionClock.Toggle(false);

        _actionsRunning = false;
    }
}
