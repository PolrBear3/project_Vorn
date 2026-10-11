using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuild_Manager : MonoBehaviour, ISubscriptionHandler, ISaveLoadable
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _deckBuildCards;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _panelToggleController;

    [SerializeField] private DeckBuild_Group[] _buildGroups;
    public DeckBuild_Group[] buildGroups => buildGroups;

    [Space(20)]
    [SerializeField] private Animator_Controller[] _progressionIcons;


    private DeckBuild_Data _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        LoadCards_toGroups();
        _panelToggleController.Toggle(true);

        _panelToggleController.OnToggle += Update_ProgressionIcons;
    }

    public void UnSubscribe_All()
    {
        _panelToggleController.OnToggle += Update_ProgressionIcons;
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("DeckBuild_Manager/DeckBuild_Data", _data);
    }

    public void Load_Data()
    {
        _data = ES3.Load("DeckBuild_Manager/DeckBuild_Data", new DeckBuild_Data());
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

    private List<Card_ScrObj> RandomLoadCards_inGroup()
    {
        int loadCardCount = MaxCardCount_inGroup();
        List<Card_ScrObj> loadCards = new();

        for (int i = 0; i < loadCardCount; i++)
        {
            loadCards.Add(_deckBuildCards[UnityEngine.Random.Range(0, _deckBuildCards.Length)]);
        }
        return loadCards;
    }
    private void LoadCards_toGroups()
    {
        _data.buildGroupCards.Clear();

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            _data.buildGroupCards.Add(RandomLoadCards_inGroup());
        }
    }

    private void Update_ProgressionIcons(bool _)
    {
        int maxCardCount = _progressionIcons.Length * MaxCardCount_inGroup();
        int currentDeckCardCount = GameManager.instance.handInventory.data.deckCardDatas.Count;

        int cardCountPerIcon = maxCardCount / _progressionIcons.Length;
        int updateCount = currentDeckCardCount / cardCountPerIcon;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {

            _progressionIcons[i].Play_State(i < updateCount ? UIAnimation.Available : UIAnimation.Restricted);
        }
    }


    // Select
    public void Select_GroupCards(DeckBuild_Group selectGroup)
    {
        HandInventory handInventory = GameManager.instance.handInventory;

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (selectGroup != _buildGroups[i]) continue;

            List<Card_ScrObj> groupCards = _data.TargetGroup_CurrentCards(i);

            foreach (Card_ScrObj card in groupCards)
            {
                handInventory.AddCard_toDeck(new(card));
            }
            break;
        }

        List<CardData> deckCards = handInventory.data.deckCardDatas;
        int totalCardCount = _progressionIcons.Length * MaxCardCount_inGroup();

        _panelToggleController.Toggle(deckCards.Count < totalCardCount);
    }
}
