using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private const float _tileSpacing = 1.0625f;

    private const float _generateXPos = 5.3125f; // -5.3125 to 5.3125 x
    private const float _generateYPos = 2.125f; // -2.125 to 2.125 y


    [Space(20)]
    [SerializeField] private GameObject _generateTilePrefab;
    [SerializeField] private Sprite[] _tileSprites;


    private List<Tile> _tiles = new();
    public List<Tile> tiles => _tiles;

    private Tile _hoveringTile;
    public Tile hoveringTile => _hoveringTile;

    private EventBus_Controller _generateEventBus = new();
    public EventBus_Controller generateEventBus => _generateEventBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Generate_Tile);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Generate_Tile);
    }


    // Generated Data
    private Tile Positioned_Tile(Vector2 position)
    {
        for (int i = 0; i < _tiles.Count; i++)
        {
            Tile currentTile = _tiles[i];

            if (currentTile.data.position != position) continue;
            return currentTile;
        }
        return null;
    }

    public List<Tile> CloseSorted_Tiles(Tile pivotTile, List<Tile> sortingTiles)
    {
        List<Tile> sortedTiles = new(sortingTiles);
        Vector2 pivotTilePos = pivotTile.data.position;

        sortingTiles.Sort((tileA, tileB) =>
        {
            int distanceA = Utility.Chebyshev_Distance(pivotTilePos, tileA.data.position);
            int distanceB = Utility.Chebyshev_Distance(pivotTilePos, tileB.data.position);

            return distanceA.CompareTo(distanceB);
        });

        return sortedTiles;
    }
    public List<Tile> PivotSurrounding_Tiles(Tile pivotTile)
    {
        Vector2 pivotTilePos = pivotTile.data.position;

        List<Vector2> surroundingPositions = Utility.Surrounding_Positions(pivotTilePos);
        List<Tile> surroundingTiles = new();

        for (int i = 0; i < surroundingPositions.Count; i++)
        {
            Tile surroundingTile = Positioned_Tile(surroundingPositions[i]);

            if (surroundingTile == null) continue;
            surroundingTiles.Add(surroundingTile);
        }
        return surroundingTiles;
    }
    public List<Tile> Distance_Tiles(Tile pivotTile, int distance)
    {
        List<Tile> distanceTiles = new(_tiles);

        for (int i = distanceTiles.Count - 1; i >= 0; i--)
        {
            Vector2 tilePos = distanceTiles[i].data.position;
            int checkDistance = Utility.Chebyshev_Distance(pivotTile.data.position, tilePos);

            if (checkDistance <= distance) continue;
            distanceTiles.RemoveAt(i);
        }
        return distanceTiles;
    }
    public List<Tile> Edged_Tiles()
    {
        List<Tile> edgedTiles = new();

        for (int i = 0; i < _tiles.Count; i++)
        {
            Tile tile = _tiles[i];

            if (PivotSurrounding_Tiles(tile).Count >= 8) continue;
            edgedTiles.Add(tile);
        }
        return edgedTiles;
    }


    // Generate
    private void Generate_Tile()
    {
        float xWorldPos = -_generateXPos;
        float yWorldPos = -_generateYPos;

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

            if (yWorldPos <= _generateYPos) continue;

            yWorldPos = -_generateYPos;
            yPos = 0;

            xWorldPos += _tileSpacing;
            xPos++;

            if (xWorldPos > _generateXPos) break;
        }
        _generateEventBus.Run_BusEvents();
    }


    // Interaction
    public void Update_hoveringTile(Tile hoveringTile)
    {
        _hoveringTile = hoveringTile;
    }
}