using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivateCondition_ScrObj : ScriptableObject
{
    public abstract bool Activate_Available(Tile activatingTile);
}
