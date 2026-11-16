using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _iconHoverController;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _deckCardCountText;
    [SerializeField] private Color _maxCapacityTextColor;

    [Space(20)]
    [SerializeField] private Image _shadowBackGround;

    [Space(20)]
    [SerializeField] private EventSystems_Controller _builtDeckPanelHoverController;
    [SerializeField] private Image _builtDeckPanel;

    [Space(20)]
    [SerializeField] private ToolTip _displayCardToolTip;
    [SerializeField] private GameObject _displayCardPrefab;


    private List<DisplayCard> _updatedDisplayCards = new();
    public List<DisplayCard> updatedDisplayCards => _updatedDisplayCards;


    // MonoBehaviour
    private void Awake()
    {
        UnToggle();

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
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += UnToggle_onInput;
        input.OnRightClick += UnToggle;

        GameManager manager = GameManager.instance;

        manager.handInventory.drawCardFromDeck.Register(0, Load_DisplayCards);
        manager.stageManager.endTurnEventBus.Register(0, UnToggle);
    }

    public void UnSubscribe_All()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= UnToggle_onInput;
        input.OnRightClick -= UnToggle;

        GameManager manager = GameManager.instance;

        manager.handInventory.drawCardFromDeck.UnRegister(Load_DisplayCards);
        manager.stageManager.endTurnEventBus.UnRegister(UnToggle);
    }


    // Hover Icon
    public void Update_DeckCardCount_Text(int updateCount)
    {
        _deckCardCountText.text = Mathf.Max(0, updateCount).ToString();

        int maxDeckCapacity = GameManager.instance.deckBuildManager.maxDeckCapacity;
        _deckCardCountText.color = updateCount >= maxDeckCapacity ? _maxCapacityTextColor : Color.white;
    }


    // Built Deck Panel
    private void Toggle(bool toggle)
    {
        toggle = toggle && Display_LoadCards().Count > 0;

        _shadowBackGround.gameObject.SetActive(toggle);
        _builtDeckPanel.gameObject.SetActive(toggle);

        _displayCardToolTip.Toggle(false);

        if (toggle == false) return;
        Load_DisplayCards();
    }
    public void Toggle()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        bool sequencesRunning = stageManager.endTurnEventBus.DelayBus_Running() || stageManager.endStageEventBus.DelayBus_Running();
        bool toggled = _builtDeckPanel.gameObject.activeSelf;

        if (toggled == false && sequencesRunning) return;
        Toggle(!toggled);
    }

    public void UnToggle()
    {
        Toggle(false);
    }
    private void UnToggle_onInput()
    {
        if (_iconHoverController.pointerDetected) return;
        if (_builtDeckPanelHoverController.pointerDetected) return;

        UnToggle();
    }

    private List<Card_ScrObj> Display_LoadCards()
    {
        GameManager manager = GameManager.instance;

        DeckBuild_Data deckBuildData = manager.deckBuildManager.data;
        if (deckBuildData.DeckUpdate_NotComplete() || manager.stageManager.stageMap.Is_BattleStage() == false) return deckBuildData.DeckCards();

        return manager.handInventory.DeckCards();
    }
    public void Load_DisplayCards()
    {
        foreach (DisplayCard removeCard in _updatedDisplayCards)
        {
            removeCard.UnSubscribe_All();
            removeCard.OnHover -= Hover_DisplayCard_ToggleToolTip;

            Destroy(removeCard.gameObject);
        }
        _updatedDisplayCards.Clear();

        Transform loadPanel = _builtDeckPanel.transform;
        List<Card_ScrObj> loadCards = Display_LoadCards();

        for (int i = 0; i < loadCards.Count; i++)
        {
            GameObject displayCardObj = Instantiate(_displayCardPrefab, loadPanel);

            if (displayCardObj.TryGetComponent(out DisplayCard displayCard) == false)
            {
                Destroy(displayCard);
                continue;
            }

            _updatedDisplayCards.Add(displayCard);

            displayCard.Subscribe_All();
            displayCard.OnHover += Hover_DisplayCard_ToggleToolTip;

            displayCard.Load_CardComponents(loadCards[i]);
        }
    }

    private List<CardData> ToolTip_CardDatas()
    {
        GameManager manager = GameManager.instance;

        DeckBuild_Data deckBuildData = manager.deckBuildManager.data;
        if (deckBuildData.DeckUpdate_NotComplete() || manager.stageManager.stageMap.Is_BattleStage() == false) return deckBuildData.Deck_CardDatas();

        return new(manager.handInventory.deckCardDatas);
    }
    private void Hover_DisplayCard_ToggleToolTip(DisplayCard hoverCard)
    {
        _displayCardToolTip.Toggle(false);

        List<CardData> toolTipCardDatas = ToolTip_CardDatas();
        if (toolTipCardDatas == null || toolTipCardDatas.Count <= 0) return;

        for (int i = 0; i < _updatedDisplayCards.Count; i++)
        {
            if (hoverCard != _updatedDisplayCards[i]) continue;

            CardData hoverData = toolTipCardDatas[i];
            Card_ScrObj cardScrObj = hoverData.cardScrObj;

            string foramtDescription = Utility.FormatUpdate_Description(hoverData.cardScrObj.cardDescription, hoverData.currentData.DescriptionFormats());

            _displayCardToolTip.ToggleOn_CursorPoint(true);
            _displayCardToolTip.Update_Contents(null, cardScrObj.contentSprite, cardScrObj.cardName, foramtDescription);

            return;
        }
    }
}