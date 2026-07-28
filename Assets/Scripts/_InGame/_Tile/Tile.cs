using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;


    private TileData _data;
    public TileData data => _data;

    private GameObject _currentPlaceable;
    public GameObject currentPlaceable => _currentPlaceable;


    // MonoBehaviour
    private void Awake()
    {
        _hoverDetector.OnPointerState += Update_OnHover;
    }

    private void OnDestroy()
    {
        _hoverDetector.OnPointerState -= Update_OnHover;
    }


    // Data
    public void Set_Data(Vector2 generatedPos)
    {
        _data = new(generatedPos);
    }


    // Card & Enemy Placeable
    public void Set_Placeable(GameObject placeableObject)
    {
        if (_currentPlaceable != null) return;
        _currentPlaceable = placeableObject;

        _currentPlaceable.transform.SetParent(transform);
        _currentPlaceable.transform.position = Vector2.zero;
    }


    // Hover
    private void Update_OnHover(bool isHovering)
    {
        GameManager.instance.tileManager.Update_hoveringTile(isHovering ? this : null);
    }
}