using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Transform _allHandCards;

    [Space(20)]
    [SerializeField][Range(0, 50)] private int _maxHandCardCount;
    [SerializeField][Range(0, 1000)] private float _handCardsSpacingValue;

    [Space(20)]
    [SerializeField] private List<Card_ScrObj> _startingDeckCards = new();


    private HandInventory_Data _data;
    public HandInventory_Data data => _data;

    private List<HandCard> _handCards = new();
    public List<HandCard> handCards => _handCards;


    private EventBus_Controller _addCardToDeckBus;
    public EventBus_Controller addCardToDeckBus => _addCardToDeckBus;

    private EventBus_Controller _drawCardFromDeck = new();
    public EventBus_Controller drawCardFromDeck => _drawCardFromDeck;


    private HandCard _hoveringCard;
    public HandCard hovaringCard => _hoveringCard;

    private HandInventory_DragDropData _dragDropData;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, LoadCards_toDeck);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, LoadCards_toDeck);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed -= Drag_HoveringCard;
        input.OnLeftClickPressed -= Drop_DraggingCard;
        input.OnRightClickPressed -= Return_DraggingCard;

        GameManager.instance.stageManager.endTurnEventBus.UnRegister(Draw_Card);
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed += Drag_HoveringCard;
        input.OnLeftClickPressed += Drop_DraggingCard;
        input.OnRightClickPressed += Return_DraggingCard;

        GameManager.instance.stageManager.endTurnEventBus.Register(0, Draw_Card);
    }

    private void LoadCards_toDeck()
    {
        _data = new(new()); // load saved data

        List<CardData> startingDeckCardDatas = new();
        foreach (Card_ScrObj card in _startingDeckCards) startingDeckCardDatas.Add(new(card));

        AddCards_toDeck(startingDeckCardDatas); // load new with shuffle
    }


    // Deck
    private void AddCard_toDeck(CardData addCardData)
    {
        if (addCardData == null) return;

        List<CardData> deckCardDatas = _data.deckCardDatas;

        int randDeckIndex = UnityEngine.Random.Range(0, deckCardDatas.Count + 1);
        deckCardDatas.Insert(randDeckIndex, addCardData);
    }
    private void AddCards_toDeck(List<CardData> addCardDatas)
    {
        if (addCardDatas == null || addCardDatas.Count <= 0) return;

        foreach (CardData cardData in addCardDatas) AddCard_toDeck(cardData);
    }


    // Hand
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

            _drawCardFromDeck.Run_BusEvents();
        }
        Update_HandCardPositions();
    }
    private void Draw_Card()
    {
        Draw_Card(1);
    }


    // HandCard Hover
    public void Update_HoveringCard(HandCard hoveringCard)
    {
        _hoveringCard = hoveringCard;
    }
    private void Drag_HoveringCard(bool isHolding)
    {
        if (_hoveringCard == null) return;
        if (isHolding == false) return;

        CardData hoveringCardData = _hoveringCard.data;
        if (GameManager.instance.cursor.Drag_Card(hoveringCardData, _hoveringCard.transform) == false) return;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (_hoveringCard != handCards[i]) continue;

            _dragDropData = new(hoveringCardData, i);
            break;
        }
        RemoveCard_fromHand(_hoveringCard);
        Update_HandCardPositions();
    }

    private bool Place_DraggingCard()
    {
        if (_dragDropData == null) return false;

        GameManager manager = GameManager.instance;
        if (manager.cardManager.PlaceCard_OnTile(_dragDropData.draggingCardData, manager.tileManager.hoveringTile) == false) return false;

        manager.cursor.Drop_Card();
        _dragDropData = null;

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
            return;
        }

        if (isHolding == false) return;
        if (Place_DraggingCard()) return;

        Return_DraggingCard();
    }

    private void Return_DraggingCard()
    {
        if (_dragDropData == null) return;

        GameManager.instance.cursor.Drop_Card();
        HandCard addedCard = AddCard_toHand(_dragDropData.draggingCardData);

        _handCards.Remove(addedCard);
        _handCards.Insert(_dragDropData.handCardsIndex, addedCard);

        _dragDropData = null;

        Update_HandCardPositions();
    }
    private void Return_DraggingCard(bool _)
    {
        Return_DraggingCard();
    }
}