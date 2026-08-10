using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTargeting_Controller : MonoBehaviour
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

    public void Target_Tile(Tile tileToTarget)
    {
        if (_targetingTiles.Contains(tileToTarget))
        {
            _targetingTiles.Remove(tileToTarget);
        }
        _targetingTiles.Add(tileToTarget);
    }
}