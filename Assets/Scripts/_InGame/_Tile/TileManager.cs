using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private const float _tileSpacing = 1.0625f;

    private const float _generateXPos = 5.3125f;
    private const float _generateYPos = -2.125f;


    [Space(20)]
    [SerializeField] private GameObject _generateTilePrefab;
    [SerializeField] private Sprite[] _tileSprites;


    private List<Tile> _tiles = new();
    public List<Tile> tiles => _tiles;

    private Tile _hoveringTile;
    public Tile hoveringTile => _hoveringTile;


    private EventBus_Controller _tileSelectBus = new();
    public EventBus_Controller tileSelectBus => _tileSelectBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Generate_Tile);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Generate_Tile);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        Input_Controller.instance.OnLeftClick -= Select_HoveringTile;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnLeftClick += Select_HoveringTile;
    }


    // Generate
    private void Generate_Tile()
    {
        float xWorldPos = _generateXPos;
        float yWorldPos = _generateYPos;

        int xPos = 0;
        int yPos = 0;

        for (int i = 0; i < 9999; i++)
        {
            GameObject spawnTile = Instantiate(_generateTilePrefab, new(xWorldPos, yWorldPos), Quaternion.identity);
            spawnTile.transform.SetParent(transform);

            if (spawnTile.TryGetComponent(out Tile tile) == false) break;
            _tiles.Add(tile);

            tile.Set_Data(new(xPos, yPos));
            tile.spriteRenderer.sprite = _tileSprites[UnityEngine.Random.Range(0, _tileSprites.Length)];

            yWorldPos += _tileSpacing;
            yPos++;

            if (yWorldPos <= _generateYPos * -1f) continue;

            yWorldPos = _generateYPos;
            yPos = 0;

            xWorldPos -= _tileSpacing;
            xPos++;

            if (xWorldPos < _generateXPos * -1f) break;
        }
    }


    // Current Grids
    public void Update_hoveringTile(Tile hoveringTile)
    {
        _hoveringTile = hoveringTile;
    }

    public bool HoveringTile_Empty()
    {
        if (_hoveringTile == null) return false;

        return _hoveringTile.currentPlaceable == null;
    }


    private void Select_HoveringTile()
    {
        if (_hoveringTile == null) return;

        _tileSelectBus.Run_BusEvents();
    }
}