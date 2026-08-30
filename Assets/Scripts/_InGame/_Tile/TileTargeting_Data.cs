using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileTargeting_Data
{
    private bool _targetingToggled;

    private List<Tile> _targetingTiles = new();
    public List<Tile> targetingTiles => _targetingTiles;

    private List<Tile> _recentTargetingTiles = new();
    public List<Tile> recentTargetingTiles => _recentTargetingTiles;


    // Targeting
    public bool Toggle_Targeting(Tile togglePivotTile)
    {
        _targetingToggled = _targetingToggled == false && togglePivotTile != null ? togglePivotTile : null;

        if (_targetingToggled == false) return false;

        _targetingTiles.Clear();
        _recentTargetingTiles.Clear();

        return true;
    }

    public void Target_Tile(Tile tileToTarget)
    {
        if (_targetingTiles.Contains(tileToTarget))
        {
            _targetingTiles.Remove(tileToTarget);
        }
        _targetingTiles.Add(tileToTarget);
        _recentTargetingTiles = new(_targetingTiles);
    }
}