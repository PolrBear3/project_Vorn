using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu
{
    private DeckModify_EventScrObj _currentModifyEvent;

    [Space(40)]
    [SerializeField] private DisplayCard[] _selectableDisplayCards;


    // MonoBehaviour
    private void Awake()
    {
        toggleController.OnToggle += Update_SelectableCards;
        OnConfirmAvailable += UpdateAmount_Selected;
    }

    private void OnDestroy()
    {
        toggleController.OnToggle -= Update_SelectableCards;
        OnConfirmAvailable -= UpdateAmount_Selected;
    }


    // EventMenu
    public override bool Update_TargetEvent(Event_ScrObj checkEvent)
    {
        if (checkEvent is not DeckModify_EventScrObj modifyEvent) return false;

        // custom toggle checks

        _currentModifyEvent = modifyEvent;
        return true;
    }

    public override void Confirm()
    {

    }
    public override void AutoConfirm()
    {

    }


    // Main
    private void Update_SelectableCards(bool toggle)
    {
        if (toggle == false) return;

        int modifyAmount = _currentModifyEvent.updateAmount;
        List<CardData> deckCardDatas = GameManager.instance.deckBuildManager.data.buildDeckCardDatas;

        int updateAmount = modifyAmount < 0
            ? Mathf.Min(Mathf.Abs(modifyAmount), deckCardDatas.Count)
            : modifyAmount;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            DisplayCard displayCard = _selectableDisplayCards[i];

            bool displayToggle = updateAmount > 0;
            displayCard.gameObject.SetActive(displayToggle);

            updateAmount--;

            if (displayToggle == false) continue;
            // displayCard.Load_Images();
        }
    }
    private bool UpdateAmount_Selected()
    {
        return true;
    }

    public void Select()
    {

    }
}