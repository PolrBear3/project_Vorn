using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckModify_EventMenu : EventMenu, ISubscriptionHandler
{
    private DeckModify_EventScrObj _currentModifyEvent;

    [Space(40)]
    [SerializeField] private GameObject _displayCardsPlatform;
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
            _selectableDisplayCards[i].OnSelect += Select;
        }
    }

    public void UnSubscribe_All()
    {
        OnConfirmAvailable -= UpdateAmount_Selected;

        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            _selectableDisplayCards[i].OnSelect -= Select;
        }
    }


    // EventMenu
    public override void AutoUpdate_forConfirm()
    {
        for (int i = 0; i < _selectableCardDatas.Count; i++)
        {
            if (UpdateAmount_Selected()) break;

            CardData autoSelectData = _selectableCardDatas[i];
            if (_selectedCardDatas.Contains(autoSelectData)) continue;

            _selectedCardDatas.Add(autoSelectData);
        }
    }
    public override void Confirm()
    {
        if (_currentModifyEvent == null) return;

        List<CardData> builtDeckCards = GameManager.instance.deckBuildManager.data.builtDeckCardDatas;
        bool isAddModify = _currentModifyEvent.modifyAmount >= 0;

        for (int i = 0; i < _selectedCardDatas.Count; i++)
        {
            CardData modifyCardData = _selectedCardDatas[i];

            if (isAddModify)
            {
                builtDeckCards.Add(modifyCardData);
                continue;
            }
            builtDeckCards.Remove(modifyCardData);
        }
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


    // Main
    private void Filter_ModifyTargetCards(List<CardData> cardDatas)
    {
        if (_currentModifyEvent == null) return;

        Card_ScrObj[] modifyCards = _currentModifyEvent.modifyTargetCards;
        if (modifyCards == null || modifyCards.Length == 0) return;

        for (int i = cardDatas.Count - 1; i >= 0; i--)
        {
            CardData cardData = cardDatas[i];
            bool containsCard = false;

            for (int j = 0; j < modifyCards.Length; j++)
            {
                if (cardData.cardScrObj != modifyCards[j]) continue;

                containsCard = true;
                break;
            }

            if (containsCard) continue;
            cardDatas.RemoveAt(i);
        }
    }

    private List<CardData> Modify_CardDatas(DeckModify_EventScrObj modifyEvent)
    {
        if (modifyEvent == null) return null;

        DeckBuild_Data deckBuildData = GameManager.instance.deckBuildManager.data;

        List<CardData> cardDatas = modifyEvent.modifyAmount >= 0
            ? deckBuildData.CardPool_CardDatas()
            : new(deckBuildData.builtDeckCardDatas);

        Filter_ModifyTargetCards(cardDatas);
        return cardDatas;
    }
    private void Update_SelectableCards()
    {
        if (_currentModifyEvent == null) return;

        List<CardData> modifyCardDatas = Modify_CardDatas(_currentModifyEvent);
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
            CardData selectableData = modifyCardDatas[randIndex];

            _selectableCardDatas.Add(selectableData);
            displayCard.Load_CardComponents(selectableData.cardScrObj);

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
        if (_selectedCardDatas.Contains(selectedCardData))
        {
            _selectedCardDatas.Remove(selectedCardData);
            return;
        }
        _selectedCardDatas.Add(selectedCardData);

        if (_selectedCardDatas.Count <= Mathf.Abs(_currentModifyEvent.modifyAmount)) return;
        _selectedCardDatas.RemoveAt(0);
    }

    public void Select(DisplayCard selectedCard)
    {
        for (int i = 0; i < _selectableDisplayCards.Length; i++)
        {
            if (selectedCard != _selectableDisplayCards[i]) continue;

            CardData selectedData = _selectableCardDatas[i];

            Update_SelectedCardDatas(selectedData);
            Update_SelectIndicationIcons();

            return;
        }
    }
}