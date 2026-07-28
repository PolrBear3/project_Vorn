using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    private Vector2 _position;
    public Vector2 position => _position;

    public TileData(Vector2 generatedPos)
    {
        _position = generatedPos;
    }
}