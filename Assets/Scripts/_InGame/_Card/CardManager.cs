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


        // from Set_Data
        GameManager.instance.tileManager.tileSelectBus.UnRegister(EventBus.AwakeLoad, DragCard_FromTile);
    }


    // Data
    private void Set_Data()
    {
        GameManager.instance.tileManager.tileSelectBus.Register(EventBus.AwakeLoad, DragCard_FromTile);
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

    private void DragCard_FromTile()
    {
        if (_dragDropData != null) return;
        
        GameManager manager = GameManager.instance;
        
        Tile hoveringTile = manager.tileManager.hoveringTile;
        if (hoveringTile == null) return;

        Vector2 dragTilePos = hoveringTile.data.position;

        CardData dragCardData = _data.PositionPlaced_CardData(dragTilePos);
        if (dragCardData == null) return;

        _dragDropData = new(dragCardData, dragTilePos);
        hoveringTile.Set_Placeable(null);

        manager.cursor.Drag_Card(dragCardData, manager.cursor.pointerIconRect);
        _data.placedCardDatas.Remove(dragCardData);
    }
}