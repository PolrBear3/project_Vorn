using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckBuild_Data
{
    [ES3Serializable] private List<List<Card_ScrObj>> _buildGroupCards = new();
    public List<List<Card_ScrObj>> buildGroupCards => _buildGroupCards;


    // Data
    public List<Card_ScrObj> TargetGroup_CurrentCards(int groupIndex)
    {
        groupIndex = Mathf.Clamp(groupIndex, 0, _buildGroupCards.Count - 1);
        return _buildGroupCards[groupIndex];
    }
}