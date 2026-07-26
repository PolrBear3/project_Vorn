using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const float _gridSpacing = 1.0625f;

    private const float _generateXPos = 5.3125f;
    private const float _generateYPos = -2.125f;


    [Space(20)]
    [SerializeField] private GameObject _generateGridPrefab;
    [SerializeField] private Sprite[] _gridSprites;

    private List<Grid> _grids = new();
    public List<Grid> grids => _grids;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Generate_Grids);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Generate_Grids);
    }


    // Generate
    private void Generate_Grids()
    {
        float xWorldPos = _generateXPos;
        float yWorldPos = _generateYPos;

        int xPos = 0;
        int yPos = 0;

        for (int i = 0; i < 9999; i++)
        {
            GameObject spawnGrid = Instantiate(_generateGridPrefab, new(xWorldPos, yWorldPos), Quaternion.identity);
            spawnGrid.transform.SetParent(transform);

            if (spawnGrid.TryGetComponent(out Grid grid) == false) break;
            _grids.Add(grid);

            grid.Set_Data(new(xPos, yPos));
            grid.spriteRenderer.sprite = _gridSprites[UnityEngine.Random.Range(0, _gridSprites.Length)];

            yWorldPos += _gridSpacing;
            yPos++;

            if (yWorldPos <= _generateYPos * -1f) continue;

            yWorldPos = _generateYPos;
            yPos = 0;

            xWorldPos -= _gridSpacing;
            xPos++;

            if (xWorldPos < _generateXPos * -1f) break;
        }
    }
}