using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileTargeting_Data
{
    private bool _targetingToggled;
    public bool targetingToggled => _targetingToggled;

    private List<Tile> _targetingTiles = new();
    public List<Tile> targetingTiles => _targetingTiles;


    // Targeting
    public void Toggle_Targeting(bool toggle)
    {
        _targetingToggled = toggle;

        if (_targetingToggled == false) return;
        _targetingTiles.Clear();
    }
    public bool Toggle_Targeting()
    {
        Toggle_Targeting(!_targetingToggled);
        return _targetingToggled;
    }

    public void Target_Tile(Tile tileToTarget)
    {
        if (_targetingTiles.Contains(tileToTarget))
        {
            _targetingTiles.Remove(tileToTarget);
        }
        _targetingTiles.Add(tileToTarget);
    }
}