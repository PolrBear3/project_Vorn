using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TilePath_Data
{
    private Tile _tile;
    public Tile tile => _tile;

    private TilePath_Data _previousPathData;
    public TilePath_Data previousPathData => _previousPathData;

    private int _gCost;
    public int gCost => _gCost;

    private int _hCost;
    public int hCost => _hCost;


    // New
    public TilePath_Data(Tile setTile, TilePath_Data previousPathData, int gCost, int hCost)
    {
        _tile = setTile;
        _previousPathData = previousPathData;

        _gCost = gCost;
        _hCost = hCost;
    }


    // Data
    public int F_Cost()
    {
        return _gCost + _hCost;
    }

    public void Update_PreviousPathData(TilePath_Data updateData)
    {
        _previousPathData = updateData;
    }
    public void UpdateG_Cost(int updateValue)
    {
        _gCost = updateValue;
    }
}
