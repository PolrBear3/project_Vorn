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

    private EventBus_Controller _tileSelectEventBus = new();
    public EventBus_Controller tileSelectEventBus => _tileSelectEventBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Generate_Tile);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Generate_Tile);


        // from Set_Data
        Input_Controller.instance.OnLeftClick -= Select_HoveringTile;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnLeftClick += Select_HoveringTile;
    }


    // Debugs
    public int Tile_Index(Tile tile)
    {
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (tile == _tiles[i]) return i;
        }
        return -1;
    }

    public string Tile_Indexes(List<Tile> tiles)
    {
        string indexString = "";

        for (int i = 0; i < tiles.Count; i++)
        {
            indexString += Tile_Index(tiles[i]);

            if (i >= tiles.Count - 1) continue;
            indexString += ", ";
        }
        return indexString;
    }


    // Generated Data
    public Tile Positioned_Tile(Vector2 position)
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

        sortedTiles.Sort((tileA, tileB) =>
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


    // Path Finding
    private TilePath_Data TargetTile_PathData(List<TilePath_Data> pathTiles, Tile targetTile)
    {
        for (int i = 0; i < pathTiles.Count; i++)
        {
            TilePath_Data data = pathTiles[i];

            if (targetTile != pathTiles[i].tile) continue;
            return data;
        }
        return null;
    }
    public List<Tile> PathFind_RouteTiles(Tile startingTile, Tile destinationTile)
    {
        List<Tile> routeTiles = new();
        if (startingTile == null || destinationTile == null) return routeTiles;

        int maxTileCount = _tiles.Count;

        List<TilePath_Data> openPaths = new();
        List<TilePath_Data> closedPaths = new();

        int startingHCost = Utility.Chebyshev_Distance(startingTile.data.position, destinationTile.data.position);
        openPaths.Add(new(startingTile, null, 0, startingHCost));

        for (int i = 0; i < maxTileCount; i++)
        {
            if (openPaths.Count <= 0) break;
            TilePath_Data currentPath = openPaths[0];

            for (int j = 0; j < openPaths.Count; j++) // comparing open paths
            {
                TilePath_Data openPath = openPaths[j];

                if (openPath == currentPath) continue;
                if (openPath.F_Cost() > currentPath.F_Cost()) continue;
                if (openPath.F_Cost() == currentPath.F_Cost() && openPath.hCost >= currentPath.hCost) continue;

                currentPath = openPath;
            }

            openPaths.Remove(currentPath);
            closedPaths.Add(currentPath);

            if (currentPath.tile == destinationTile) // destination reached
            {
                while (currentPath.previousPathData != null)
                {
                    routeTiles.Add(currentPath.tile);
                    currentPath = currentPath.previousPathData;
                }
                routeTiles.Reverse();
                return routeTiles;
            }

            List<Tile> surroundingTiles = PivotSurrounding_Tiles(currentPath.tile);

            for (int j = 0; j < surroundingTiles.Count; j++) // add open paths
            {
                Tile surroundingTile = surroundingTiles[j];

                if (surroundingTile.currentOccupant != null) continue;
                if (TargetTile_PathData(closedPaths, surroundingTile) != null) continue;

                int distanceToSurroundingTile = Utility.Chebyshev_Distance(currentPath.tile.data.position, surroundingTile.data.position);
                int gCost = currentPath.gCost + distanceToSurroundingTile;

                TilePath_Data openPathData = TargetTile_PathData(openPaths, surroundingTile);

                if (openPathData == null)
                {
                    int hCost = Utility.Chebyshev_Distance(surroundingTile.data.position, destinationTile.data.position);
                    openPaths.Add(new(surroundingTile, currentPath, gCost, hCost));

                    continue;
                }

                if (gCost >= openPathData.gCost) continue;

                openPathData.Update_PreviousPathData(currentPath);
                openPathData.UpdateG_Cost(gCost);
            }
        }
        return routeTiles;
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
            spawnTile.name = spawnTile.name + " " + i;

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
        _generateEventBus.RunSequential_BusEvents();
    }


    // Interaction
    public void Update_hoveringTile(Tile hoveringTile)
    {
        _hoveringTile = hoveringTile;
    }

    public void Select_HoveringTile()
    {
        if (_hoveringTile == null) return;

        _tileSelectEventBus.RunSequential_BusEvents();
    }
}