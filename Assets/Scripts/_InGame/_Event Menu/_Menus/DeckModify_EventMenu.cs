using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu, ISubscriptionHandler
{
    private DeckModify_EventScrObj _currentModifyEvent;

    [Space(40)]
    [SerializeField] private GameObject _displayCardsPlatform;

    [Space(20)]
    [SerializeField] private ToolTip _selectableCardHoverToolTip;
    [SerializeField] private DisplayCard[] _selectableDisplayCards;

    [Space(20)]
    [SerializeField] private Sprite[] _selectSprites;


    private List<CardData> _selectableCardDatas = new();
    private List<CardData> _selectedCardDatas = new();


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        OnConfirmAvailable += UpdateAmount_Selected;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            DisplayCard displayCard = _selectableDisplayCards[i];

            displayCard.OnHover += Hover_SelectableCard;
            displayCard.OnSelect += Select;
        }
    }

    public void UnSubscribe_All()
    {
        OnConfirmAvailable -= UpdateAmount_Selected;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            DisplayCard displayCard = _selectableDisplayCards[i];

            displayCard.OnHover -= Hover_SelectableCard;
            displayCard.OnSelect -= Select;
        }
    }


    // EventMenu
    public override void AutoUpdate_forConfirm()
    {
        for (int i = 0; i < _selectableCardDatas.Count; i++)
        {
            if (UpdateAmount_Selected()) break;

            CardData autoSelectedData = _selectableCardDatas[i];
            if (_selectedCardDatas.Contains(autoSelectedData)) continue;

            _selectedCardDatas.Add(autoSelectedData);
        }
    }
    public override void Confirm()
    {
        if (_currentModifyEvent == null) return;

        DeckBuild_Manager deckBuildManager = GameManager.instance.deckBuildManager;
        DeckBuild_Data deckBuildData = deckBuildManager.data;

        bool isAddModify = _currentModifyEvent.modifyAmount >= 0;

        for (int i = 0; i < _selectedCardDatas.Count; i++)
        {
            Card_ScrObj modifyCard = _selectedCardDatas[i].cardScrObj;

            if (isAddModify)
            {
                deckBuildData.AddCard_toBuiltDeck(modifyCard, 1);
                continue;
            }
            deckBuildData.RemoveCard_fromBuiltDeck(modifyCard, 1);
        }
        deckBuildManager.deckManager.Update_DeckCardCount_Text(deckBuildData.builtDeckCards.Count);
    }

    public override bool Update_TargetEvent(Event_ScrObj checkEvent)
    {
        if (checkEvent is not DeckModify_EventScrObj modifyEvent) return false;
        if (modifyEvent.updateAmount <= 0 || modifyEvent.modifyAmount == 0) return false;

        _currentModifyEvent = modifyEvent;

        _selectableCardDatas.Clear();
        _selectedCardDatas.Clear();

        Update_SelectableCards();
        Update_SelectIndicationIcons();

        return true;
    }


    // UI
    private Sprite SelectIndication_Sprite()
    {
        if (_currentModifyEvent == null) return null;

        int spriteIndex = _currentModifyEvent.modifyAmount >= 0 ? 0 : 1;
        return _selectSprites[spriteIndex];
    }
    private void Update_SelectIndicationIcons()
    {
        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            CardData selectableData = i <= _selectableCardDatas.Count - 1 ? _selectableCardDatas[i] : null;
            Sprite updateSprite = _selectedCardDatas.Contains(selectableData) ? SelectIndication_Sprite() : null;

            _selectableDisplayCards[i].Load_IndicationIcon(updateSprite);
        }
    }

    private void Hover_SelectableCard(DisplayCard hoverCard)
    {
        _selectableCardHoverToolTip.ToggleOn_CursorPoint(hoverCard != null);

        if (hoverCard == null) return;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            if (hoverCard != _selectableDisplayCards[i]) continue;

            Card_ScrObj hoveringCard = _selectableCardDatas[i].cardScrObj;

            string foramtDescription = Utility.FormatUpdate_Description(hoveringCard.cardDescription, hoveringCard.interactionData.DescriptionFormats());
            _selectableCardHoverToolTip.Update_Contents(null, hoveringCard.contentSprite, hoveringCard.cardName, foramtDescription);

            return;
        }
    }


    // Main
    private void Filter_ModifyTargetCards(List<Card_ScrObj> filterTargetCards)
    {
        if (_currentModifyEvent == null) return;

        Card_ScrObj[] modifyCards = _currentModifyEvent.modifyTargetCards;
        if (modifyCards == null || modifyCards.Length == 0) return;

        for (int i = filterTargetCards.Count - 1; i >= 0; i--)
        {
            Card_ScrObj filterCard = filterTargetCards[i];
            bool containsCard = false;

            for (int j = 0; j < modifyCards.Length; j++)
            {
                if (filterCard != modifyCards[j]) continue;

                containsCard = true;
                break;
            }

            if (containsCard) continue;
            filterTargetCards.RemoveAt(i);
        }
    }
    private List<Card_ScrObj> ModifyCards(DeckModify_EventScrObj modifyEvent)
    {
        if (modifyEvent == null) return null;

        DeckBuild_Data deckBuildData = GameManager.instance.deckBuildManager.data;

        List<Card_ScrObj> cardDatas = modifyEvent.modifyAmount >= 0
            ? deckBuildData.CardPool_Cards()
            : new(deckBuildData.builtDeckCards);

        Filter_ModifyTargetCards(cardDatas);
        return cardDatas;
    }

    private void Update_SelectableCards()
    {
        if (_currentModifyEvent == null) return;

        List<Card_ScrObj> modifyCardDatas = ModifyCards(_currentModifyEvent);
        int updateAmount = Mathf.Min(_currentModifyEvent.updateAmount, modifyCardDatas.Count, _selectableDisplayCards.Length);

        bool togglePlatform = updateAmount > 0;
        _displayCardsPlatform.SetActive(togglePlatform);

        if (togglePlatform == false) return;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            DisplayCard displayCard = _selectableDisplayCards[i];

            bool displayToggle = i < updateAmount;
            displayCard.gameObject.SetActive(displayToggle);

            if (displayToggle == false) continue;

            int randIndex = UnityEngine.Random.Range(0, modifyCardDatas.Count);
            Card_ScrObj selectableCard = modifyCardDatas[randIndex];

            _selectableCardDatas.Add(new(selectableCard));
            displayCard.Load_CardComponents(selectableCard);

            modifyCardDatas.RemoveAt(randIndex);
        }
    }

    private bool UpdateAmount_Selected()
    {
        int selectedAmount = _selectedCardDatas.Count;

        if (selectedAmount >= _selectableCardDatas.Count) return true;
        return selectedAmount >= Mathf.Abs(_currentModifyEvent.modifyAmount);
    }
    private void Update_SelectedCardDatas(CardData selectedCardData)
    {
        if (_selectableCardDatas.Contains(selectedCardData) == false) return;

        if (_selectedCardDatas.Contains(selectedCardData))
        {
            _selectedCardDatas.Remove(selectedCardData);
            return;
        }
        _selectedCardDatas.Add(selectedCardData);

        if (_selectedCardDatas.Count <= Mathf.Abs(_currentModifyEvent.modifyAmount)) return;
        _selectedCardDatas.RemoveAt(0);
    }

    public void Select(DisplayCard selectedDisplayCard)
    {
        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            if (selectedDisplayCard != _selectableDisplayCards[i]) continue;

            Update_SelectedCardDatas(_selectableCardDatas[i]);
            Update_SelectIndicationIcons();

            return;
        }
    }
}
