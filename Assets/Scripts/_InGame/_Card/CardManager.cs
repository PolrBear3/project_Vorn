using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager_DragDropData
{
    private CardData _draggingCardData;
    public CardData draggingCardData => _draggingCardData;

    private Vector2 _draggedTilePosition;
    public Vector2 draggedTilePosition => _draggedTilePosition;

    private bool _draggedOnClick;
    public bool draggedOnClick => _draggedOnClick;


    // New
    public CardManager_DragDropData(CardData draggingCardData, Vector2 draggedTilePos)
    {
        _draggingCardData = draggingCardData;
        _draggedTilePosition = draggedTilePos;
        _draggedOnClick = true;
    }

    // Data
    public void DragComplete()
    {
        _draggedOnClick = false;
    }
}

public class CardManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _cardPrefab;
    

    private CardManager_Data _data = new();
    public CardManager_Data data => _data;

    private CardManager_DragDropData _dragDropData;
    public CardManager_DragDropData dragDropData => _dragDropData;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    private void Set_Data()
    {

    }


    // Cards
    public bool PlaceCard_OnTile(CardData placeCardData, Tile placeTile)
    {
        if (placeCardData == null || placeCardData.cardScrObj == null) return false;
        if (placeTile == null || placeTile.currentPlaceable != null) return false;

        GameObject placeCardObj = Instantiate(_cardPrefab);
        placeTile.Set_Placeable(placeCardObj);

        if (placeCardObj.TryGetComponent(out Card placeCard) == false) return false;

        placeCard.Load(placeCardData, placeTile);
        _data.Add_PlacedData(placeCard);

        return true;
    }
}