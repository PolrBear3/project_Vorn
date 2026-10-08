using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_Manager : MonoBehaviour, ISubscriptionHandler, ISaveLoadable
{
    [Space(20)]
    [SerializeField] private Card_ScrObj[] _deckBuildCards;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _panelToggleController;

    [SerializeField] private DeckBuild_Group[] _buildGroups;
    public DeckBuild_Group[] buildGroups => buildGroups;


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
    }

    public void UnSubscribe_All()
    {

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


    // Select
    public void Select_GroupCards(DeckBuild_Group selectGroup)
    {
        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (selectGroup != _buildGroups[i]) continue;

            List<Card_ScrObj> groupCards = _data.TargetGroup_CurrentCards(i);
            Debug.Log(i + "    " + groupCards.Count);

            return;
        }
    }
}
