using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuildGroup_ToolTip : ToolTip
{
    [Space(40)]
    [SerializeField] private DisplayCard[] _groupCards;
    public DisplayCard[] groupCards => _groupCards;
}