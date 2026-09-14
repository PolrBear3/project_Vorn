using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [SerializeField] private Animator_Controller _indicatorAnimController;
    public Animator_Controller indicatorAnimController => _indicatorAnimController;


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


    // Occupant
    public bool Set_Occupant(GameObject occupantObject)
    {
        if (occupantObject == null)
        {
            _currentOccupant = null;
            return true;
        }

        if (_currentOccupant != null) return false;

        _currentOccupant = occupantObject;
        return true;
    }

    public IInteractable CurrentOccupant_Interactable()
    {
        if (_currentOccupant == null) return null;
        if (_currentOccupant.TryGetComponent(out IInteractable interactable) == false) return null;

        return interactable;
    }


    // Hover
    private void Update_OnHover(bool isHovering)
    {
        GameManager.instance.tileManager.Update_hoveringTile(isHovering ? this : null);
    }
}