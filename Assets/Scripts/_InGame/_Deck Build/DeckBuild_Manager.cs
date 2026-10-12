using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuild_Manager : MonoBehaviour, ISubscriptionHandler, ISaveLoadable
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _defaultCardPool;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _panelToggleController;

    [SerializeField] private DeckBuild_Group[] _buildGroups;
    public DeckBuild_Group[] buildGroups => buildGroups;

    [Space(20)]
    [SerializeField] private Animator_Controller[] _progressionIcons;


    private List<Card_ScrObj> _cardPool = new();
    public List<Card_ScrObj> cardPool => _cardPool;

    public Action OnCardPoolLoad;


    private DeckBuild_Data _data;
    public DeckBuild_Data data => _data;

    private EventBus_Controller _deckBuildCompleteBus;
    public EventBus_Controller deckBuildCompleteBus => _deckBuildCompleteBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.StartLoad, Toggle_DeckBuild);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);

        EventBus_GlobalController.UnRegister(Toggle_DeckBuild);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        _panelToggleController.OnToggle += Update_ProgressionIcons;

        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.setStageEventBus.Register(DeckBuild_InComplete);
        stageManager.setStageEventBus.Register(0, Add_BuiltDeckCards_toDeck);
    }

    public void UnSubscribe_All()
    {
        _panelToggleController.OnToggle += Update_ProgressionIcons;

        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.setStageEventBus.UnRegister(DeckBuild_InComplete);
        stageManager.setStageEventBus.UnRegister(Add_BuiltDeckCards_toDeck);
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("DeckBuild_Manager/DeckBuild_Data", _data, SaveKeys.GamePlayFile);
    }

    public void Load_Data()
    {
        _data = ES3.Load("DeckBuild_Manager/DeckBuild_Data", SaveKeys.GamePlayFile, new DeckBuild_Data());
    }


    // Load
    private int MaxCardCount_inGroup()
    {
        int maxCount = int.MinValue;

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            int count = _buildGroups[i].groupCards.Length;
            if (count <= maxCount) continue;

            maxCount = count;
        }
        return maxCount;
    }

    public bool DeckBuild_Complete()
    {
        int totalCardCount = _progressionIcons.Length * MaxCardCount_inGroup();
        return _data.buildDeckCardDatas.Count >= totalCardCount;
    }
    private bool DeckBuild_InComplete()
    {
        return DeckBuild_Complete() == false;
    }


    private void Load_CardPool()
    {
        for (int i = 0; i < _defaultCardPool.Length; i++)
        {
            cardPool.Add(_defaultCardPool[i]);
        }
        OnCardPoolLoad?.Invoke();
    }

    private List<Card_ScrObj> Random_CardPoolCards_inGroup()
    {
        List<Card_ScrObj> loadCards = new();
        if (_cardPool == null || _cardPool.Count <= 0) return loadCards;

        int loadCardCount = MaxCardCount_inGroup();

        for (int i = 0; i < loadCardCount; i++)
        {
            loadCards.Add(_cardPool[UnityEngine.Random.Range(0, _cardPool.Count)]);
        }
        return loadCards;
    }
    private void Load_CardPoolCards_toGroups()
    {
        _data.buildGroupCards.Clear();

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            _data.buildGroupCards.Add(Random_CardPoolCards_inGroup());

            List<Card_ScrObj> loadCards = _data.TargetGroup_CurrentCards(i);
            DeckBuild_GroupCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                groupCards[j].Load_Images(loadCards[j]);
            }
        }
    }

    private void Toggle_DeckBuild()
    {
        if (DeckBuild_Complete()) return;

        Load_CardPool();
        Load_CardPoolCards_toGroups();

        _panelToggleController.Toggle(true);
    }
    private void Update_ProgressionIcons(bool _)
    {
        int maxCardCount = _progressionIcons.Length * MaxCardCount_inGroup();
        int currentDeckCardCount = _data.buildDeckCardDatas.Count;

        int cardCountPerIcon = maxCardCount / _progressionIcons.Length;
        int updateCount = currentDeckCardCount / cardCountPerIcon;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {

            _progressionIcons[i].Play_State(i < updateCount ? UIAnimation.Available : UIAnimation.Restricted);
        }
    }

    private void Add_BuiltDeckCards_toDeck()
    {
        GameManager.instance.handInventory.AddCards_toDeck(new(_data.buildDeckCardDatas));
    }


    // Select
    public void Select_GroupCards(DeckBuild_Group selectGroup)
    {
        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (selectGroup != _buildGroups[i]) continue;

            List<Card_ScrObj> groupCards = _data.TargetGroup_CurrentCards(i);

            foreach (Card_ScrObj card in groupCards)
            {
                _data.buildDeckCardDatas.Add(new(card));
            }
            break;
        }

        bool deckBuildComplete = DeckBuild_Complete();
        _panelToggleController.Toggle(deckBuildComplete == false);

        if (deckBuildComplete == false)
        {
            Load_CardPoolCards_toGroups();
            return;
        }
        GameManager.instance.Continue_GamePlay();
    }
}