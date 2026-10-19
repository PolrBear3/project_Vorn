using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_Group : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private DisplayCard[] _groupCards;
    public DisplayCard[] groupCards => _groupCards;


    // Button
    public void Select()
    {
        GameManager.instance.deckBuildManager.Select_GroupCards(this);
    }
}