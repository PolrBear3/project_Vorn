using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ISaveLoadable
{
    public static GameManager instance;


    private GameData _gameData;
    public GameData gameData => _gameData;


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
    [SerializeField] private EnvironmentManager _environmentManager;
    public EnvironmentManager environmentManager => _environmentManager;

    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [SerializeField] private DeckBuild_Manager _deckBuildManager;
    public DeckBuild_Manager deckBuildManager => _deckBuildManager;

    [SerializeField] private HandInventory _handInventory;
    public HandInventory handInventory => _handInventory;

    [SerializeField] private EventMenu_Manager _eventMenuManager;
    public EventMenu_Manager eventMenuManager => _eventMenuManager;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventBus_GlobalController.Run_BusEvents();
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("GameManager/GameData", gameData, SaveKeys.ProgressionFile);
    }

    public void Load_Data()
    {
        _gameData = ES3.Load("GameManager/GameData", SaveKeys.ProgressionFile, new GameData());
    }


    // Global
    private void Reload_GameScene()
    {
        EventBus_GlobalController.UnSubscribeAll_SubscriptionHandlers();

        // Screen Hide Animation ?
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Continue_GamePlay()
    {
        SaveLoad_Controller.instance.SaveAll_ISaveLoadable();
        Reload_GameScene();
    }
    public void Reset_GamePlay()
    {
        ES3.DeleteFile(SaveKeys.GamePlayFile);
        Reload_GameScene();
    }
}