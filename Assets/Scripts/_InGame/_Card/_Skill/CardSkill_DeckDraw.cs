using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSkill_DeckDraw : CardSkill
{
    [Space(40)]
    [SerializeField][Range(0, 100)] private int _drawAmount;


    // CardSkill
    public override IEnumerator Activate_Skill()
    {
        HandInventory handInventory = GameManager.instance.handInventory;
        yield return handInventory.DrawCard();
    }
}