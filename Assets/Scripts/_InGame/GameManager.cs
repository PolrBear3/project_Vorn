using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    [Space(20)]
    [SerializeField] private TileManager _tileManager;
    public TileManager tileManager => _tileManager;

    [SerializeField] private CardManager _cardManager;
    public CardManager cardManager => _cardManager;

    [SerializeField] private StageManager _stageManager;
    public StageManager stageManager => _stageManager;

    [SerializeField] private HeroManager _heroManager;
    public HeroManager heroManager => _heroManager;

    [SerializeField] private EnemyManager _enemyManager;
    public EnemyManager enemyManager => _enemyManager;

    [Space(10)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [SerializeField] private HandInventory _handInventory;
    public HandInventory handInventory => _handInventory;


    [Space(20)]
    [SerializeField] private GameData _newGameData;

    private GameData _currentGameData;
    public GameData currentGameData => _currentGameData;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;

        EventBus_GlobalController.Register(EventBus.AwakeLoad, Load_GameData);
    }

    private void Start()
    {
        EventBus_GlobalController.Run_BusEvents();
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Load_GameData);
    }


    // Load Game
    private void Load_GameData()
    {
        _currentGameData = _newGameData;
    }
}