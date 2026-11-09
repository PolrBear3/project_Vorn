using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_Group : HoverUI
{
    [Space(40)]
    [SerializeField] private DisplayCard[] _groupCards;
    public DisplayCard[] groupCards => _groupCards;


    // HoverUI
    public override void OnHover_Update(bool _)
    {

    }

    public override void OnSelect_Update()
    {
        GameManager.instance.deckBuildManager.Select_GroupCards(this);
    }
}