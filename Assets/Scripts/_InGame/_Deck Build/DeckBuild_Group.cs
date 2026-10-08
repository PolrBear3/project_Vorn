using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_Group : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private DeckBuild_GroupCard[] _groupCards;
    public DeckBuild_GroupCard[] groupCards => _groupCards;


    // Button
    public void Select()
    {
        GameManager.instance.deckBuildManager.Select_GroupCards(this);
    }
}