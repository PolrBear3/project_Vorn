using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

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
    private IEnumerator Reload_GameScene()
    {
        EventBus_GlobalController.UnSubscribeAll_SubscriptionHandlers();

        _environmentManager.Toggle_TransitionScreen(true);
        yield return null;
        while (_environmentManager.TransitionScreen_Playing()) yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Continue_GamePlay()
    {
        SaveLoad_Controller.instance.SaveAll_ISaveLoadable();
        StartCoroutine(Reload_GameScene());
    }
    public void Reset_GamePlay()
    {
        Save_Data();

        ES3.DeleteFile(SaveKeys.GamePlayFile);
        StartCoroutine(Reload_GameScene());
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class GameManager_Inspector : Editor
{
    private Card_ScrObj _cardToAddToDeck;
    private bool _deckAddGenerated = true;

    private Card_ScrObj _cardToAddToHand;
    private bool _handAddGenerated = true;

    public override void OnInspectorGUI()
    {
        GameManager controller = (GameManager)target;

        base.OnInspectorGUI();
        serializedObject.Update();

        GUILayout.Space(60);

        EditorGUILayout.BeginHorizontal();

        _cardToAddToDeck = (Card_ScrObj)EditorGUILayout.ObjectField(_cardToAddToDeck, typeof(Card_ScrObj), false);
        _deckAddGenerated = GUILayout.Toggle(_deckAddGenerated, "Generated", GUILayout.Width(85));

        using (new EditorGUI.DisabledScope(!Application.isPlaying || _cardToAddToDeck == null ||
        controller.handInventory == null || controller.deckBuildManager == null))
        {
            if (GUILayout.Button("Add Card to Deck", GUILayout.Width(120)))
            {
                HandInventory inventory = controller.handInventory;
                inventory.AddCard_toDeck(new CardData(_cardToAddToDeck, _deckAddGenerated));

                DeckManager deckManager = controller.deckBuildManager.deckManager;
                deckManager.Update_DeckCardCount_Text(inventory.deckCardDatas.Count);
                deckManager.Load_DisplayCards();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Draw Card"))
        {
            controller.handInventory.DrawingStateCheck_DrawCard();
        }

        EditorGUILayout.BeginHorizontal();

        _cardToAddToHand = (Card_ScrObj)EditorGUILayout.ObjectField(_cardToAddToHand, typeof(Card_ScrObj), false);
        _handAddGenerated = GUILayout.Toggle(_handAddGenerated, "Generated", GUILayout.Width(85));

        using (new EditorGUI.DisabledScope(!Application.isPlaying || _cardToAddToHand == null ||
        controller.handInventory == null))
        {
            if (GUILayout.Button("Add Card to Hand", GUILayout.Width(120)))
            {
                HandInventory inventory = controller.handInventory;
                inventory.AddCard_toHand(new CardData(_cardToAddToHand, _handAddGenerated));
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Damage All Enemies"))
        {
            List<Enemy> spawnedEnemies = controller.enemyManager.spawnedEnemies;

            foreach (Enemy enemy in spawnedEnemies)
            {
                enemy.data.currentData.Update_CurrentHealth(0, out int _);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
