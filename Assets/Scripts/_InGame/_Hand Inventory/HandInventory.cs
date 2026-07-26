using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private EventBus_Controller _addCardEventBus = new();
    public EventBus_Controller addCardEventBus => _addCardEventBus;

    private HandInventory_Data _data;
    public HandInventory_Data data => _data;

    private List<HandCard> _handCards = new();
    public List<HandCard> handCards => _handCards;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, LoadCards_toDeck);

        Input_Controller.instance.OnHoldInteract += _addCardEventBus.Run_BusEvents;
        _addCardEventBus.Register(EventBus.AwakeLoad, Draw_Card);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, LoadCards_toDeck);

        Input_Controller.instance.OnHoldInteract -= _addCardEventBus.Run_BusEvents;
        _addCardEventBus.UnRegister(EventBus.AwakeLoad, Draw_Card);
    }


    // Deck
    private void LoadCards_toDeck()
    {
        _data = new(new()); // load saved data

        List<CardData> startingDeckCardDatas = new();
        foreach (Card_ScrObj card in _startingDeckCards) startingDeckCardDatas.Add(new(card));

        AddCards_toDeck(startingDeckCardDatas); // load new with shuffle
    }

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
    public void AddCard_toHand(CardData addCardData)
    {
        if (_handCards.Count >= _maxHandCardCount) return;

        GameObject addCardObject = Instantiate(_handCardPrefab, _allHandCards);
        if (addCardObject.TryGetComponent(out HandCard addCard) == false) return;

        addCard.Load(addCardData);

        _handCards.Add(addCard);
        _data.handCardDatas.Add(addCard.data);

        Update_HandCardPositions();
    }
    private void RemoveCard_fromHand(int removeIndex)
    {
        if (_handCards == null || _handCards.Count <= 0) return;

        removeIndex = Mathf.Clamp(removeIndex, 0, _handCards.Count - 1);
        HandCard removeCard = _handCards[removeIndex];

        _handCards.RemoveAt(removeIndex);
        _data.handCardDatas.Remove(removeCard.data);

        Destroy(removeCard.gameObject);
        Update_HandCardPositions();
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
        }
    }
    private void Draw_Card()
    {
        Draw_Card(1);
    }
}
