using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomData_Utility
{
    public static bool AllConditions_ActivateAvailable(Tile activatingTile, ActivateCondition_ScrObj[] conditions)
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
}
