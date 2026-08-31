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

public class HandInventory : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _handCardPrefab;

    [Space(10)]
    [SerializeField] private Transform _allHandCards;
    [SerializeField] private Image _cardPlatform;

    [Space(20)]
    [SerializeField][Range(0, 1000)] private float _handCardsSpacingValue;
    [SerializeField][Range(0, 1000)] private float _platformWidthUpdateValue;

    private float _defaultPlatformWidth;

    [Space(10)]
    [SerializeField][Range(0, 50)] private int _maxHandCardCount;


    private HandInventory_Data _data;
    public HandInventory_Data data => _data;

    private List<HandCard> _handCards = new();
    public List<HandCard> handCards => _handCards;


    private HandCard _hoveringCard;
    public HandCard hovaringCard => _hoveringCard;

    private HandInventory_DragDropData _dragDropData;
    public HandInventory_DragDropData dragDropData => _dragDropData;


    private EventBus_Controller _addCardToDeckBus;
    public EventBus_Controller addCardToDeckBus => _addCardToDeckBus;

    private EventBus_Controller _drawCardFromDeck = new();
    public EventBus_Controller drawCardFromDeck => _drawCardFromDeck;


    // MonoBehaviour
    private void Awake()
    {
        _defaultPlatformWidth = _cardPlatform.rectTransform.rect.width;

        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.Register(EventBus.StartLoad, LoadCards_toDeck);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.UnRegister(EventBus.StartLoad, LoadCards_toDeck);


        // from Set_Data
        GameManager manager = GameManager.instance;
        
        manager.tileManager.tileHoverEventBus.Register(0, HoverTile_DraggingCard);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed -= Drag_HoveringCard;
        input.OnLeftClickPressed -= Drop_DraggingCard;
        input.OnRightClickPressed -= Return_DraggingCard;

        StageManager stageManager = manager.stageManager;
        stageManager.stageSetEventBus.UnRegister(DrawCard_Delay);

        EventBus_Controller endTurnEventBus = stageManager.endTurnEventBus;

        endTurnEventBus.UnRegister(Return_DraggingCard);
        endTurnEventBus.UnRegister(DrawCard_Delay);
    }


    // Data
    private void Set_Data()
    {
        _data = new(new()); // load saved data

        GameManager manager = GameManager.instance;

        _cardPlatform.sprite = manager.currentGameData.hero.cardPlatformSprite;
        Update_CardPlatform();


        manager.tileManager.tileHoverEventBus.Register(0, HoverTile_DraggingCard);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed += Drag_HoveringCard;
        input.OnLeftClickPressed += Drop_DraggingCard;
        input.OnRightClickPressed += Return_DraggingCard;

        StageManager stageManager = manager.stageManager;
        stageManager.stageSetEventBus.Register(1, DrawCard_Delay);

        EventBus_Controller endTurnEventBus = stageManager.endTurnEventBus;

        endTurnEventBus.Register(0, Return_DraggingCard);
        endTurnEventBus.Register(2, DrawCard_Delay);
    }

    private void LoadCards_toDeck()
    {
        GameData currentGameData = GameManager.instance.currentGameData;

        AddCards_toDeck(currentGameData.DeckCard_Datas());
        AddCard_toTopDeck(new(currentGameData.hero.spawnCard));
    }


    // Deck
    public void AddCard_toDeck(CardData addCardData)
    {
        if (addCardData == null || addCardData.cardScrObj == null) return;

        List<CardData> deckCardDatas = _data.deckCardDatas;

        int randDeckIndex = UnityEngine.Random.Range(0, deckCardDatas.Count + 1);
        deckCardDatas.Insert(randDeckIndex, addCardData);
    }
    public void AddCards_toDeck(List<CardData> addCardDatas)
    {
        if (addCardDatas == null || addCardDatas.Count <= 0) return;

        foreach (CardData cardData in addCardDatas) AddCard_toDeck(cardData);
    }

    public void AddCard_toTopDeck(CardData addCardData)
    {
        if (addCardData == null || addCardData.cardScrObj == null) return;

        _data.deckCardDatas.Add(addCardData);
    }


    // Hand
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
        bool toggle = currentCardCount > 0;

        _cardPlatform.gameObject.SetActive(toggle);
        if (toggle == false) return;

        float updateWidthValue = _defaultPlatformWidth + Mathf.Max(0, (currentCardCount - 2) * _platformWidthUpdateValue);
        _cardPlatform.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, updateWidthValue);
    }

    public HandCard AddCard_toHand(CardData addCardData)
    {
        if (_handCards.Count >= _maxHandCardCount) return null;

        GameObject addCardObject = Instantiate(_handCardPrefab, _allHandCards);
        if (addCardObject.TryGetComponent(out HandCard addCard) == false) return null;

        addCard.Load(addCardData);

        _handCards.Add(addCard);
        _data.handCardDatas.Add(addCard.data);

        return addCard;
    }
    private void RemoveCard_fromHand(HandCard removeCard)
    {
        if (_handCards == null || _handCards.Count <= 0) return;

        for (int i = 0; i < _handCards.Count; i++)
        {
            HandCard cardToRemove = _handCards[i];
            if (removeCard != cardToRemove) continue;

            _handCards.RemoveAt(i);
            _data.handCardDatas.Remove(cardToRemove.data);

            Destroy(cardToRemove.gameObject);
            break;
        }
    }

    public void Draw_Card(int drawCount)
    {
        List<CardData> deckCardDatas = _data.deckCardDatas;
        if (deckCardDatas == null || deckCardDatas.Count <= 0) return;

        for (int i = 0; i < drawCount; i++)
        {
            int drawCardIndex = deckCardDatas.Count - 1;
            CardData drawCardData = deckCardDatas[drawCardIndex];

            deckCardDatas.RemoveAt(drawCardIndex);
            AddCard_toHand(drawCardData);

            _drawCardFromDeck.RunSequential_BusEvents();
        }

        Update_HandCardPositions();
        Update_CardPlatform();
    }
    private void Draw_Card()
    {
        Draw_Card(1);
    }

    private IEnumerator DrawCard_Delay()
    {
        // draw card lean tween effect ?

        Draw_Card();
        yield break;
    }


    // HandCard Hover
    public void Update_HoveringCard(HandCard hoveringCard)
    {
        _hoveringCard = hoveringCard;
    }
    private void Drag_HoveringCard(bool isHolding)
    {
        if (isHolding == false || _hoveringCard == null) return;

        GameManager manager = GameManager.instance;
        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;

        CardData hoveringCardData = _hoveringCard.data;
        if (manager.cursor.Drag_Card(hoveringCardData, _hoveringCard.transform) == false) return;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (_hoveringCard != handCards[i]) continue;

            _dragDropData = new(hoveringCardData, i);
            break;
        }
        RemoveCard_fromHand(_hoveringCard);

        Update_HandCardPositions();
        Update_CardPlatform();
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

    private bool Place_DraggingCard()
    {
        if (_dragDropData == null) return false;

        GameManager manager = GameManager.instance;
        CardManager cardManager = manager.cardManager;

        if (cardManager.CardPlace_ActionRunning()) return false;
        if (cardManager.PlaceCard_OnTile(_dragDropData.draggingCardData, manager.tileManager.hoveringTile) == false) return false;

        manager.cursor.Drop_Card();

        _dragDropData = null;
        manager.tileManager.Reset_TileIndicators();

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