using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_Manager : MonoBehaviour, ISubscriptionHandler, ISaveLoadable
{
    [Space(20)]
    [SerializeField] private DeckManager _deckManager;
    public DeckManager deckManager => _deckManager;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _panelToggleController;

    [Space(10)]
    [SerializeField] private ToolTip _groupCardHoverToolTip;
    [SerializeField] private DeckBuild_Group[] _buildGroups;
    public DeckBuild_Group[] buildGroups => buildGroups;

    [Space(10)]
    [SerializeField] private DeckBuildGroup_ToolTip _progressionIconHoverHoverToolTip;
    [SerializeField] private DeckBuild_ProgressionIcon[] _progressionIcons;

    [Space(20)]
    [SerializeField] private Card_ScrObj[] _defaultCardPool;


    private DeckBuild_Data _data;
    public DeckBuild_Data data => _data;


    public Action OnCardPoolLoad;

    private EventBus_Controller _deckUpdateCompleteBus = new();
    public EventBus_Controller deckUpdateCompleteBus => _deckUpdateCompleteBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.StartLoad, Resume_DeckBuild);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);

        EventBus_GlobalController.UnRegister(Resume_DeckBuild);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        stageManager.stageMap.maptoggleController.OnToggle += UpdateCurrent_DeckCardCount_Text;

        stageManager.setStageEventBus.Register(_data.DeckUpdate_NotComplete);
        stageManager.endStageEventBus.Register(_data.DeckUpdate_NotComplete);

        manager.heroManager.heroSelectManager.heroSelectCompleteBus.Register(0, Begin_DeckBuild);
        stageManager.endStageEventBus.Register(0, DeckUpdate_onStageComplete);

        _panelToggleController.OnToggle += Load_DeckBuild;

        _panelToggleController.OnToggle += Update_ProgressionIcons;
        _deckUpdateCompleteBus.Register(0, Update_ProgressionIcons);

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                groupCards[j].OnHover += Hover_GroupCards;
            }
        }
    }

    public void UnSubscribe_All()
    {
        GameManager manager = GameManager.instance;

        StageManager stageManager = manager.stageManager;
        stageManager.stageMap.maptoggleController.OnToggle -= UpdateCurrent_DeckCardCount_Text;

        stageManager.setStageEventBus.UnRegister(_data.DeckUpdate_NotComplete);
        stageManager.endStageEventBus.UnRegister(_data.DeckUpdate_NotComplete);

        manager.heroManager.heroSelectManager.heroSelectCompleteBus.UnRegister(Begin_DeckBuild);
        stageManager.endStageEventBus.UnRegister(DeckUpdate_onStageComplete);

        _panelToggleController.OnToggle -= Load_DeckBuild;

        _panelToggleController.OnToggle -= Update_ProgressionIcons;
        _deckUpdateCompleteBus.UnRegister(Update_ProgressionIcons);

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                groupCards[j].OnHover -= Hover_GroupCards;
            }
        }
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
    private List<Card_ScrObj> DeckUpdateCards_inGroupIndex(int index)
    {
        int cardsPerGroup = MaxCardCount_inGroup();
        int startIndex = Mathf.Max(0, index) * cardsPerGroup;

        List<Card_ScrObj> deckUpdateCards = new(_data.deckUpdateCards);
        List<Card_ScrObj> groupCards = new();

        for (int i = startIndex; i < deckUpdateCards.Count; i++)
        {
            if (groupCards.Count >= cardsPerGroup) break;
            groupCards.Add(deckUpdateCards[i]);
        }
        return groupCards;
    }


    private void Load_CardPool()
    {
        _data.cardPool.Clear();

        for (int i = 0; i < _defaultCardPool.Length; i++)
        {
            _data.cardPool.Add(_defaultCardPool[i]);
        }
        OnCardPoolLoad?.Invoke();
    }

    private List<Card_ScrObj> Random_CardPoolCards_inGroup()
    {
        List<Card_ScrObj> loadCards = new();

        List<Card_ScrObj> cardPool = new(_data.cardPool);
        if (cardPool == null || cardPool.Count <= 0) return loadCards;

        int loadCardCount = MaxCardCount_inGroup();

        for (int i = 0; i < loadCardCount; i++)
        {
            loadCards.Add(cardPool[UnityEngine.Random.Range(0, cardPool.Count)]);
        }
        return loadCards;
    }
    private void Load_CardPoolCards_toGroups()
    {
        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (i >= _data.buildGroupCards.Count) // load new cards if group is empty
            {
                _data.buildGroupCards.Add(Random_CardPoolCards_inGroup());
            }

            List<Card_ScrObj> loadCards = _data.TargetGroup_CurrentCards(i);
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                groupCards[j].Load_CardComponents(loadCards[j]);
            }
        }
    }


    private void Load_DeckBuild(bool deckBuildToggled)
    {
        if (deckBuildToggled == false) return;

        Load_CardPool();
        Load_CardPoolCards_toGroups();

        UpdateCurrent_DeckCardCount_Text();
    }

    private void Begin_DeckBuild()
    {
        Hero_ScrObj selectedHero = GameManager.instance.heroManager.data.selectedHero;
        if (selectedHero == null) return;

        _data.Toggle_DeckUpdate(selectedHero.deckUpdateSelectCount * MaxCardCount_inGroup());
        _panelToggleController.Toggle(true);
    }
    private void Resume_DeckBuild()
    {
        if (_data.deckUpdateCompleteCount <= 0) return;

        _panelToggleController.Toggle(true);
    }

    private void DeckUpdate_onStageComplete()
    {
        StageMap_Manager stageMap = GameManager.instance.stageManager.stageMap;

        if (stageMap.Selecting_Stage() || stageMap.Stage_NotSelected() == false) return;
        if (stageMap.data.FinalLevel_Complete()) return;

        Begin_DeckBuild();
    }


    // Interaction
    private void UpdateCurrent_DeckCardCount_Text()
    {
        _deckManager.Update_DeckCardCount_Text(_data.builtDeckCards.Count + _data.deckUpdateCards.Count);
    }
    private void UpdateCurrent_DeckCardCount_Text(bool _)
    {
        UpdateCurrent_DeckCardCount_Text();
    }

    private void Update_ProgressionIcons(bool deckBuildToggled)
    {
        if (deckBuildToggled == false) return;

        int cardsPerGroup = MaxCardCount_inGroup();

        int progressionCount = _data.deckUpdateCompleteCount / cardsPerGroup;
        int completionCount = _data.deckUpdateCards.Count / cardsPerGroup;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {
            DeckBuild_ProgressionIcon icon = _progressionIcons[i];

            bool toggle = i < progressionCount;
            icon.gameObject.SetActive(toggle);

            if (toggle == false) continue;
            icon.Update_UnlockVisuals(i < completionCount);
        }
    }
    private void Update_ProgressionIcons()
    {
        for (int i = 0; i < _progressionIcons.Length; i++)
        {
            DeckBuild_ProgressionIcon icon = _progressionIcons[i];

            if (icon.gameObject.activeSelf == false) continue;
            icon.Update_UnlockVisuals(true);
        }
    }

    private void Hover_GroupCards(DisplayCard hoverCard)
    {
        bool toggle = hoverCard != null && _data.DeckUpdate_NotComplete();
        _groupCardHoverToolTip.ToggleOn_CursorPoint(toggle);

        if (toggle == false) return;

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                if (hoverCard != groupCards[j]) continue;

                List<Card_ScrObj> currentGroupCards = _data.TargetGroup_CurrentCards(i);
                Card_ScrObj hoveringCard = currentGroupCards[j];

                string foramtDescription = Utility.FormatUpdate_Description(hoveringCard.cardDescription, hoveringCard.interactionData.DescriptionFormats());
                _groupCardHoverToolTip.Update_Contents(null, hoveringCard.contentSprite, hoveringCard.cardName, foramtDescription);

                return;
            }
        }
    }
    private void HoverCurrent_GroupCards()
    {
        for (int i = 0; i < _buildGroups.Length; i++)
        {
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                DisplayCard groupCard = groupCards[j];
                if (groupCard.hoverDetector.pointerDetected == false) continue;

                Hover_GroupCards(groupCard);
                return;
            }
        }
    }

    public void Hover_ProgressionIcon(DeckBuild_ProgressionIcon hoverIcon)
    {
        _progressionIconHoverHoverToolTip.UnToggle();

        if (hoverIcon == null) return;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {
            DeckBuild_ProgressionIcon icon = _progressionIcons[i];
            if (hoverIcon != icon) continue;

            List<Card_ScrObj> deckUpdateCards = DeckUpdateCards_inGroupIndex(i);
            if (deckUpdateCards == null || deckUpdateCards.Count <= 0) return;

            DisplayCard[] tooltipCards = _progressionIconHoverHoverToolTip.groupCards;
            for (int j = 0; j < deckUpdateCards.Count; j++)
            {
                if (j > tooltipCards.Length - 1) break;
                tooltipCards[j].Load_CardComponents(deckUpdateCards[j]);
            }

            RectTransform toolTipPanel = _progressionIconHoverHoverToolTip.panel.rectTransform;
            toolTipPanel.position = new(icon.image.rectTransform.position.x, toolTipPanel.position.y);

            _progressionIconHoverHoverToolTip.Toggle(true);
            return;
        }
    }
    public void Select_GroupCards(DeckBuild_Group selectGroup)
    {
        if (_data.deckUpdateCompleteCount <= 0) return;

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (selectGroup != _buildGroups[i]) continue;

            List<Card_ScrObj> groupCards = _data.TargetGroup_CurrentCards(i);

            foreach (Card_ScrObj card in groupCards)
            {
                _data.deckUpdateCards.Add(card);
            }
            break;
        }

        UpdateCurrent_DeckCardCount_Text();
        _deckManager.Load_DeckCards();

        if (_data.Complete_DeckUpdate() == false)
        {
            _data.buildGroupCards.Clear();
            Load_CardPoolCards_toGroups();

            _panelToggleController.Toggle(true);
            HoverCurrent_GroupCards();

            return;
        }
        _groupCardHoverToolTip.Toggle(false);

        _deckUpdateCompleteBus.RunSequential_BusEvents();
        StartCoroutine(_deckUpdateCompleteBus.RunSequential_DelayBusEvents());

        GameManager manager = GameManager.instance;

        if (manager.stageManager.stageMap.Stage_NotSelected())
        {
            _panelToggleController.Toggle(false);
            return;
        }
        manager.Continue_GamePlay();
    }
}
