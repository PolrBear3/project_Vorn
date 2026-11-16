using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandInventory_DragDropData
{
    private CardData _draggingCardData;
    public CardData draggingCardData => _draggingCardData;

    private int _handCardsIndex;
    public int handCardsIndex => _handCardsIndex;

    private bool _draggedOnClick;
    public bool draggedOnClick => _draggedOnClick;


    // New
    public HandInventory_DragDropData(CardData draggingCardData, int cardIndex)
    {
        _draggingCardData = draggingCardData;
        _handCardsIndex = cardIndex;
        _draggedOnClick = true;
    }

    // Data
    public void DragComplete()
    {
        _draggedOnClick = false;
    }
}

public class HandInventory : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private GameObject _handCardPrefab;
    [SerializeField] private Transform _handCardsPositionPoint;

    [Space(20)]
    [SerializeField] private Image _cardPlatform;
    [SerializeField] private UIPanel_ToggleController _cardPlatformToggler;

    [Space(20)]
    [SerializeField][Range(0, 1000)] private float _handCardsSpacingValue;
    [SerializeField][Range(0, 1000)] private float _platformWidthUpdateValue;

    [Space(10)]
    [SerializeField][Range(0, 50)] private int _maxHandCardCount;

    [Space(20)]
    [SerializeField] private ToolTip _handCardHoverToolTip;


    private List<CardData> _deckCardDatas = new();
    public List<CardData> deckCardDatas => _deckCardDatas;

    private List<HandCard> _handCards = new();
    public List<HandCard> handCards => _handCards;

    private Coroutine _drawCardCoroutine;


    private HandCard _hoveringCard;
    public HandCard hovaringCard => _hoveringCard;

    private HandInventory_DragDropData _dragDropData;
    public HandInventory_DragDropData dragDropData => _dragDropData;


    private float _defaultPlatformWidth;
    public Action<float> OnPlatformWidthUpdate;


    private EventBus_Controller _drawCardFromDeck = new();
    public EventBus_Controller drawCardFromDeck => _drawCardFromDeck;

    private EventBus_Controller _placeCardEventBus = new();
    public EventBus_Controller placeCardEventBus => _placeCardEventBus;


    // MonoBehaviour
    private void Awake()
    {
        _defaultPlatformWidth = _cardPlatform.rectTransform.rect.width;

        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    //ISubscriptionHandler
    public void Subscribe_All()
    {
        GameManager manager = GameManager.instance;

        manager.tileManager.tileHoverEventBus.Register(0, HoverTile_DraggingCard);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed += Drag_HoveringCard;
        input.OnLeftClickPressed += Drop_DraggingCard;
        input.OnRightClickPressed += Return_DraggingCard;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller setStage = stageManager.setStageEventBus;

        setStage.Register(0, LoadHero_CardPlatform);
        setStage.Register(1, Load_BuiltDeckCards_toDeck);

        EventBus_Controller endTurnEventBus = stageManager.endTurnEventBus;

        endTurnEventBus.Register(0, Return_DraggingCard);
        endTurnEventBus.Register(5, DrawCard);
    }

    public void UnSubscribe_All()
    {
        GameManager manager = GameManager.instance;

        manager.tileManager.tileHoverEventBus.UnRegister(HoverTile_DraggingCard);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed -= Drag_HoveringCard;
        input.OnLeftClickPressed -= Drop_DraggingCard;
        input.OnRightClickPressed -= Return_DraggingCard;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller setStage = stageManager.setStageEventBus;

        setStage.UnRegister(LoadHero_CardPlatform);
        setStage.UnRegister(Load_BuiltDeckCards_toDeck);

        EventBus_Controller endTurnEventBus = stageManager.endTurnEventBus;

        endTurnEventBus.UnRegister(DrawCard);
        endTurnEventBus.UnRegister(Return_DraggingCard);
    }


    // Deck
    public List<Card_ScrObj> DeckCards()
    {
        List<Card_ScrObj> cards = new();

        foreach (CardData data in deckCardDatas)
        {
            cards.Add(data.cardScrObj);
        }
        return cards;
    }

    public void AddCard_toDeck(CardData addCardData)
    {
        if (addCardData == null || addCardData.cardScrObj == null) return;
        if (_deckCardDatas.Count >= GameManager.instance.deckBuildManager.maxDeckCapacity) return;

        _deckCardDatas.Add(addCardData);
    }
    public void AddCards_toDeck(List<CardData> addCardDatas)
    {
        if (addCardDatas == null || addCardDatas.Count <= 0) return;
        foreach (CardData cardData in addCardDatas) AddCard_toDeck(cardData);
    }

    private void Load_BuiltDeckCards_toDeck()
    {
        DeckBuild_Manager deckBuildManager = GameManager.instance.deckBuildManager;

        AddCards_toDeck(deckBuildManager.data.Deck_CardDatas());
        deckBuildManager.deckManager.Update_DeckCardCount_Text(_deckCardDatas.Count);
    }


    // Visual
    private void LoadHero_CardPlatform()
    {
        Hero_ScrObj currentHero = GameManager.instance.heroManager.data.selectedHero;
        if (currentHero == null) return;

        _cardPlatform.sprite = currentHero.cardPlatformSprite;
    }

    private void Update_HandCardPositions()
    {
        if (_handCards == null || _handCards.Count <= 0) return;

        float totalWidth = (_handCards.Count - 1) * _handCardsSpacingValue;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < _handCards.Count; i++)
        {
            float xPos = startX + (i * _handCardsSpacingValue);
            _handCards[i].rectTransform.anchoredPosition = new Vector2(xPos, 0f);
        }
    }
    private void Update_CardPlatform()
    {
        int currentCardCount = _handCards.Count;

        StageMap_Manager stageMap = GameManager.instance.stageManager.stageMap;
        bool gameStateToggle = stageMap.Stage_NotSelected() == false && stageMap.Is_BattleStage();

        _cardPlatformToggler.Toggle(currentCardCount > 0 && gameStateToggle);

        float updateWidthValue = _defaultPlatformWidth + Mathf.Max(0, (currentCardCount - 2) * _platformWidthUpdateValue);
        updateWidthValue = currentCardCount > 0 ? updateWidthValue : 0f;

        _cardPlatform.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, updateWidthValue);
        OnPlatformWidthUpdate?.Invoke(_cardPlatform.rectTransform.rect.width);
    }


    // Hand
    public HandCard AddCard_toHand(CardData addCardData)
    {
        if (_handCards.Count >= _maxHandCardCount) return null;

        GameObject addCardObject = Instantiate(_handCardPrefab, _handCardsPositionPoint);

        if (addCardObject.TryGetComponent(out HandCard addCard) == false) return null;
        _handCards.Add(addCard);

        addCard.Load(new CardData(addCardData));

        Update_HandCardPositions();
        Update_CardPlatform();

        return addCard;
    }
    public void RemoveCard_fromHand(HandCard removeCard)
    {
        if (_handCards == null || _handCards.Count <= 0) return;

        for (int i = 0; i < _handCards.Count; i++)
        {
            HandCard cardToRemove = _handCards[i];
            if (removeCard != cardToRemove) continue;

            _handCards.RemoveAt(i);
            Destroy(cardToRemove.gameObject);

            break;
        }

        Update_HandCardPositions();
        Update_CardPlatform();
    }

    private CardData DrawCardData()
    {
        if (_deckCardDatas == null || _deckCardDatas.Count <= 0) return null;

        for (int i = 0; i < _deckCardDatas.Count; i++)
        {
            if (_deckCardDatas[i].drawPrioritized == false) continue;
            return _deckCardDatas[i];
        }
        return _deckCardDatas[UnityEngine.Random.Range(0, _deckCardDatas.Count)];
    }
    private void DrawCard_UpdateDeck()
    {
        CardData drawCardData = DrawCardData();
        if (drawCardData == null) return;

        if (AddCard_toHand(drawCardData) == null) return;
        _deckCardDatas.Remove(drawCardData);

        GameManager.instance.deckBuildManager.deckManager.Update_DeckCardCount_Text(_deckCardDatas.Count);
    }


    private IEnumerator DrawCard(int drawCount)
    {
        if (_drawCardFromDeck.RunCondition_Available() == false) yield break;

        for (int i = 0; i < drawCount; i++)
        {
            DrawCard_UpdateDeck();

            _drawCardFromDeck.RunSequential_BusEvents();
            yield return _drawCardFromDeck.RunSequential_DelayBusEvents();

            while (_drawCardFromDeck.DelayBus_Running()) yield return null;
        }
    }
    public IEnumerator DrawCard()
    {
        yield return DrawCard(1);
    }

    public void DrawingStateCheck_DrawCard()
    {
        if (_drawCardCoroutine != null)
        {
            StopCoroutine(_drawCardCoroutine);
            _drawCardCoroutine = null;
        }
        _drawCardCoroutine = StartCoroutine(DrawCard());
    }


    // Hover
    public void Update_HoveringCard(HandCard hoveringCard)
    {
        _hoveringCard = hoveringCard;

        // ToolTip
        bool toggle = hoveringCard != null && _dragDropData == null;
        _handCardHoverToolTip.Toggle(toggle);

        if (toggle == false) return;

        CardData hoverCardData = hoveringCard.cardData;
        Card_ScrObj hoverCardScrObj = hoverCardData.cardScrObj;

        string foramtDescription = Utility.FormatUpdate_Description(hoverCardScrObj.cardDescription, hoverCardData.currentData.DescriptionFormats());
        _handCardHoverToolTip.Update_Contents(null, hoverCardScrObj.contentSprite, hoverCardScrObj.cardName, foramtDescription);

        RectTransform panel = _handCardHoverToolTip.panel.rectTransform;
        panel.anchoredPosition = new(hoveringCard.rectTransform.anchoredPosition.x, panel.anchoredPosition.y);
    }
    private void Drag_HoveringCard(bool isHolding)
    {
        if (isHolding == false || _hoveringCard == null) return;

        GameManager manager = GameManager.instance;
        StageManager stageManager = manager.stageManager;

        if (stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (stageManager.stageMap.Stage_NotSelected()) return;

        CardData hoveringCardData = _hoveringCard.cardData;
        if (manager.cursor.Drag_Card(hoveringCardData, _hoveringCard.transform) == false) return;

        for (int i = 0; i < _handCards.Count; i++)
        {
            if (_hoveringCard != _handCards[i]) continue;

            _dragDropData = new(hoveringCardData, i);
            break;
        }
        RemoveCard_fromHand(_hoveringCard);
    }

    private void HoverTile_DraggingCard()
    {
        if (_dragDropData == null) return;

        TileManager tileManager = GameManager.instance.tileManager;
        tileManager.Reset_TileIndicators();

        Tile hoveringTile = tileManager.hoveringTile;
        if (hoveringTile == null) return;

        string playState = hoveringTile.currentOccupant == null ? UIAnimation.Toggle : UIAnimation.Restricted;
        hoveringTile.indicatorAnimController.Play_State(playState);
    }


    // Drag Drop
    private bool Place_DraggingCard()
    {
        if (_dragDropData == null) return false;

        GameManager manager = GameManager.instance;
        CardManager cardManager = manager.cardManager;

        if (cardManager.CardPlace_ActionRunning()) return false;

        CardData dragCardData = _dragDropData.draggingCardData;

        HeroManager heroManager = manager.heroManager;
        int cardManaCost = dragCardData.currentManaPrice;

        if (heroManager.Current_ManaCount() < cardManaCost)
        {
            // not enough mana panel animation ? 
            return false;
        }

        if (cardManager.PlaceCard_OnTile(dragCardData, manager.tileManager.hoveringTile) == false) return false;
        heroManager.Modify_CurrentManaCount(-cardManaCost);

        manager.cursor.Drop_Card();
        _dragDropData = null;

        manager.tileManager.Reset_TileIndicators();

        _placeCardEventBus.RunSequential_BusEvents();
        return true;
    }
    private void Drop_DraggingCard(bool isHolding)
    {
        if (_dragDropData == null) return;

        if (_dragDropData.draggedOnClick)
        {
            if (isHolding) return;
            if (Place_DraggingCard()) return;

            _dragDropData.DragComplete();
            GameManager.instance.tileManager.Reset_TileIndicators();

            return;
        }

        if (isHolding == false) return;
        if (Place_DraggingCard()) return;

        Return_DraggingCard();
    }

    private void Return_DraggingCard()
    {
        if (_dragDropData == null) return;

        GameManager manager = GameManager.instance;

        manager.cursor.Drop_Card();
        HandCard addedCard = AddCard_toHand(_dragDropData.draggingCardData);

        _handCards.Remove(addedCard);
        _handCards.Insert(_dragDropData.handCardsIndex, addedCard);

        _dragDropData = null;
        manager.tileManager.Reset_TileIndicators();

        Update_HandCardPositions();
        Update_CardPlatform();
    }
    private void Return_DraggingCard(bool _)
    {
        Return_DraggingCard();
    }
}
