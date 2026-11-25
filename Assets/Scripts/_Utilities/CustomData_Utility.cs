using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomData_Utility
{
    public static bool Conditions_ActivateAvailable(Tile activatingTile, ActivateCondition_ScrObj[] conditions)
    {
        for (int i = 0; i < conditions.Length; i++)
        {
            ActivateCondition_ScrObj condition = conditions[i];

            if (condition == null) continue;
            if (condition.Activate_Available(activatingTile)) continue;

            return false;
        }
        return true;
    }
    public static List<Tile> Conditions_ActivateAvailableTiles(List<Tile> activatingTiles, ActivateCondition_ScrObj[] conditions)
    {
        List<Tile> availableTiles = new();

        for (int i = 0; i < activatingTiles.Count; i++)
        {
            Tile activatingTile = activatingTiles[i];

            if (Conditions_ActivateAvailable(activatingTile, conditions) == false) continue;
            availableTiles.Add(activatingTile);
        }
        return availableTiles;
    }
}
