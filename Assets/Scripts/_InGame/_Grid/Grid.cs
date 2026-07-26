using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    [Space(20)]
    [SerializeField] private EventSystems_Controller _eventSystems;


    private GridData _data;
    public GridData data => _data;

    private GameObject _currentPlaceable;
    public GameObject currentPlaceable => _currentPlaceable;


    // MonoBehaviour
    private void Awake()
    {

    }

    private void OnDestroy()
    {

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
}