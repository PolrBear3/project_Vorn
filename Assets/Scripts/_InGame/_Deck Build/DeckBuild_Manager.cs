using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private EventBus_Controller _deckBuildCompleteBus = new();
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
        GameManager manager = GameManager.instance;

        manager.heroManager.heroSelectManager.heroSelectCompleteBus.Register(0, Toggle_DeckBuild);
        _panelToggleController.OnToggle += Update_ProgressionIcons;

        StageManager stageManager = manager.stageManager;

        stageManager.setStageEventBus.Register(DeckBuild_InComplete);
        stageManager.setStageEventBus.Register(0, Add_BuiltDeckCards_toDeck);

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

        manager.heroManager.heroSelectManager.heroSelectCompleteBus.UnRegister(Toggle_DeckBuild);
        _panelToggleController.OnToggle += Update_ProgressionIcons;

        StageManager stageManager = manager.stageManager;

        stageManager.setStageEventBus.UnRegister(DeckBuild_InComplete);
        stageManager.setStageEventBus.UnRegister(Add_BuiltDeckCards_toDeck);

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
    private bool DeckBuild_InComplete()
    {
        return _data.deckBuildComplete == false;
    }


    private void Load_CardPool()
    {
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
        _data.buildGroupCards.Clear();

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            _data.buildGroupCards.Add(Random_CardPoolCards_inGroup());

            List<Card_ScrObj> loadCards = _data.TargetGroup_CurrentCards(i);
            DisplayCard[] groupCards = _buildGroups[i].groupCards;

            for (int j = 0; j < groupCards.Length; j++)
            {
                groupCards[j].Load_CardComponents(loadCards[j]);
            }
        }
    }

    private void Toggle_DeckBuild()
    {
        if (GameManager.instance.heroManager.data.selectedHero == null) return;
        if (_data.deckBuildComplete) return;

        Load_CardPool();
        Load_CardPoolCards_toGroups();

        _deckManager.Update_DeckCardCount_Text(_data.builtDeckCardDatas.Count);
        _panelToggleController.Toggle(true);
    }
    private void Update_ProgressionIcons(bool _)
    {
        int maxCardCount = _progressionIcons.Length * MaxCardCount_inGroup();
        int currentDeckCardCount = _data.builtDeckCardDatas.Count;

        int cardCountPerIcon = maxCardCount / _progressionIcons.Length;
        int updateCount = currentDeckCardCount / cardCountPerIcon;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {
            _progressionIcons[i].Update_UnlockVisuals(i < updateCount);
        }
    }

    private List<CardData> BuiltDeckCards_inGroupIndex(int index)
    {
        int cardsPerGroup = MaxCardCount_inGroup();
        int startIndex = Mathf.Max(0, index) * cardsPerGroup;

        List<CardData> builtCardDatas = new(_data.builtDeckCardDatas);
        List<CardData> builtGroupCardDatas = new();

        for (int i = startIndex; i < builtCardDatas.Count; i++)
        {
            if (builtGroupCardDatas.Count >= cardsPerGroup) break;
            builtGroupCardDatas.Add(builtCardDatas[i]);
        }
        return builtGroupCardDatas;
    }
    private void Add_BuiltDeckCards_toDeck()
    {
        HandInventory handInventory = GameManager.instance.handInventory;

        handInventory.AddCards_toDeck(new(_data.builtDeckCardDatas));
        _deckManager.Update_DeckCardCount_Text(handInventory.dekcCards.Count);
    }


    // Interaction
    private void Hover_GroupCards(DisplayCard hoverCard)
    {
        bool toggle = hoverCard != null && _data.deckBuildComplete == false;
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

    public void Select_GroupCards(DeckBuild_Group selectGroup)
    {
        if (_data.deckBuildComplete) return;

        for (int i = 0; i < _buildGroups.Length; i++)
        {
            if (selectGroup != _buildGroups[i]) continue;

            List<Card_ScrObj> groupCards = _data.TargetGroup_CurrentCards(i);

            foreach (Card_ScrObj card in groupCards)
            {
                _data.builtDeckCardDatas.Add(new(card));
            }
            break;
        }
        _deckManager.Update_DeckCardCount_Text(_data.builtDeckCardDatas.Count);

        int buildCardCount = _progressionIcons.Length * MaxCardCount_inGroup();
        if (_data.Complete_DeckBuild(buildCardCount) == false)
        {
            _panelToggleController.Toggle(true);

            Load_CardPoolCards_toGroups();
            HoverCurrent_GroupCards();

            return;
        }
        _groupCardHoverToolTip.Toggle(false);

        GameManager.instance.Continue_GamePlay();
    }

    public void Hover_ProgressionIcon(DeckBuild_ProgressionIcon hoverIcon)
    {
        _progressionIconHoverHoverToolTip.UnToggle();

        if (hoverIcon == null) return;

        for (int i = 0; i < _progressionIcons.Length; i++)
        {
            DeckBuild_ProgressionIcon icon = _progressionIcons[i];
            if (hoverIcon != icon) continue;

            List<CardData> builtCards = BuiltDeckCards_inGroupIndex(i);
            if (builtCards == null || builtCards.Count <= 0) return;

            DisplayCard[] tooltipCards = _progressionIconHoverHoverToolTip.groupCards;
            for (int j = 0; j < builtCards.Count; j++)
            {
                if (j > tooltipCards.Length - 1) break;
                tooltipCards[j].Load_CardComponents(builtCards[j].cardScrObj);
            }

            RectTransform toolTipPanel = _progressionIconHoverHoverToolTip.panel.rectTransform;
            toolTipPanel.position = new(icon.image.rectTransform.position.x, toolTipPanel.position.y);

            _progressionIconHoverHoverToolTip.Toggle(true);
            return;
        }
    }
}
