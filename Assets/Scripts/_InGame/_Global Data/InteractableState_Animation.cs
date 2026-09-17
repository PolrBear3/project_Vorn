using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractableState_Animation
{
    public const string Taunt = "Taunt";
    public const string Shield = "Shield";
    public const string Frozen = "Frozen";

    public static string InteractableState_AnimationName(InteractableState state, bool toggle)
    {
        string removeString = toggle ? null : "Remove";

        switch (state)
        {
            case InteractableState.Taunt: 
                return Taunt + removeString;

            case InteractableState.Shield: 
                return Shield + removeString;

            case InteractableState.Frozen: 
                return Frozen + removeString;
        }
        return null;
    }
}