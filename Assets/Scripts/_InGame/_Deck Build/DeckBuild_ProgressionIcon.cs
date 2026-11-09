using System.Collections;
using System.Collections.Generic;
using Plugins.Animate_UI_Materials;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuild_ProgressionIcon : HoverUI
{
    // HoverUI
    public override void OnHover_Update(bool isHovering)
    {
        GameManager.instance.deckBuildManager.Hover_ProgressionIcon(isHovering ? this : null);
    }

    public override void OnSelect_Update()
    {

    }
}
