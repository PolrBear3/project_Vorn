using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    [Space(20)]
    [SerializeField] private TileManager _tileManager;
    public TileManager tileManager => _tileManager;

    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [SerializeField] private HandInventory _handInventory;
    public HandInventory handInventory => _handInventory;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventBus_GlobalController.Run_BusEvents();
    }
}