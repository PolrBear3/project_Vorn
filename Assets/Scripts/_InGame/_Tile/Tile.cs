using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;


    private TileData _data;
    public TileData data => _data;

    private GameObject _currentOccupant;
    public GameObject currentOccupant => _currentOccupant;


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
    public void Set_Occupant(GameObject occupantObject)
    {
        if (occupantObject == null)
        {
            Destroy(_currentOccupant);
            _currentOccupant = null;
            
            return;
        }
        if (_currentOccupant != null) return;
        
        _currentOccupant = occupantObject;
        _currentOccupant.transform.SetParent(transform);
        _currentOccupant.transform.localPosition = Vector2.zero;
    }


    // Hover
    private void Update_OnHover(bool isHovering)
    {
        GameManager.instance.tileManager.Update_hoveringTile(isHovering ? this : null);
    }
}