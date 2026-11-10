using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelect_Manager : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private ToolTip _heroHoverToolTip;
    [SerializeField] private ToolTip _spawnCarddHoverToolTip;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _toggleController;
    [SerializeField] private Hero_SelectGroup[] _selectGroups;

    [Space(20)]
    [SerializeField] private Hero_ScrObj[] _selectableHeroPool;


    private EventBus_Controller _heroSelectCompleteBus = new();
    public EventBus_Controller heroSelectCompleteBus => _heroSelectCompleteBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.StartLoad, Toggle_HeroSelect);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);

        EventBus_GlobalController.UnRegister(Toggle_HeroSelect);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        for (int i = 0; i < _selectGroups.Length; i++)
        {
            Hero_SelectGroup selectGroup = _selectGroups[i];

            selectGroup.OnHeroHover += Hover_Hero;
            selectGroup.OnSpawnCardHover += Hover_SpawnCard;
        }
    }

    public void UnSubscribe_All()
    {
        for (int i = 0; i < _selectGroups.Length; i++)
        {
            Hero_SelectGroup selectGroup = _selectGroups[i];

            selectGroup.OnHeroHover -= Hover_Hero;
            selectGroup.OnSpawnCardHover -= Hover_SpawnCard;
        }
    }


    // Hero Selecting
    public List<Hero_ScrObj> New_SelectableHeroes()
    {
        List<Hero_ScrObj> availableHeroes = new(_selectableHeroPool);
        List<Hero_ScrObj> selectableHeroes = new();

        for (int i = 0; i < _selectGroups.Length; i++)
        {
            if (availableHeroes.Count <= 0)
            {
                selectableHeroes.Add(_selectableHeroPool[UnityEngine.Random.Range(0, _selectableHeroPool.Length)]);
                continue;
            }

            int randIndex = UnityEngine.Random.Range(0, availableHeroes.Count);

            selectableHeroes.Add(availableHeroes[randIndex]);
            availableHeroes.RemoveAt(randIndex);
        }
        return selectableHeroes;
    }


    private Hero_ScrObj Hovering_Hero(Hero_SelectGroup hoveringGroup)
    {
        for (int i = 0; i < _selectGroups.Length; i++)
        {
            if (hoveringGroup != _selectGroups[i]) continue;
            return GameManager.instance.heroManager.data.selectableHeros[i];
        }
        return null;
    }

    private void Hover_Hero(Hero_SelectGroup hovergroup)
    {
        Hero_ScrObj hoveringHero = Hovering_Hero(hovergroup);
        bool toggle = hoveringHero != null && GameManager.instance.heroManager.data.selectedHero == null;

        _heroHoverToolTip.ToggleOn_CursorPoint(toggle);

        if (toggle == false) return;
        _heroHoverToolTip.Update_Contents(hoveringHero.toolTipBaseSprite, null, hoveringHero.characterName, hoveringHero.characterDescription);
    }
    private void Hover_SpawnCard(Hero_SelectGroup hovergroup)
    {
        Hero_ScrObj hoveringHero = Hovering_Hero(hovergroup);
        bool toggle = hoveringHero != null && GameManager.instance.heroManager.data.selectedHero == null;

        _spawnCarddHoverToolTip.ToggleOn_CursorPoint(toggle);

        if (toggle == false) return;
        Card_ScrObj spawnCard = hoveringHero.spawnCard;

        string foramtDescription = Utility.FormatUpdate_Description(spawnCard.cardDescription, spawnCard.interactionData.DescriptionFormats());
        _spawnCarddHoverToolTip.Update_Contents(null, spawnCard.contentSprite, spawnCard.cardName, foramtDescription);
    }


    private void Toggle_HeroSelect()
    {
        GameManager manager = GameManager.instance;

        HeroManager_Data data = manager.heroManager.data;
        if (data.selectedHero != null) return;

        List<Hero_ScrObj> selectableHeroes = new(data.selectableHeros);

        for (int i = 0; i < _selectGroups.Length; i++)
        {
            Hero_SelectGroup selectGroup = _selectGroups[i];
            Hero_ScrObj selectHero = selectableHeroes[i];

            selectGroup.Update_SelectHero(selectHero);
            selectGroup.Update_Coins(manager.gameData.Hero_ProgressionCount(selectHero));
        }
        _toggleController.Toggle(true);
    }
    public void Select_Hero(Hero_SelectGroup selectGroup)
    {
        HeroManager_Data data = GameManager.instance.heroManager.data;
        if (data.selectedHero != null) return;

        for (int i = 0; i < _selectGroups.Length; i++)
        {
            Hero_SelectGroup heroSelectGroup = _selectGroups[i];
            heroSelectGroup.Toggle_HoverLock(true);

            if (selectGroup != heroSelectGroup) continue;

            data.Update_SelectedHero(data.selectableHeros[i]);

            _heroHoverToolTip.Toggle(false);
            _spawnCarddHoverToolTip.Toggle(false);

            _toggleController.Toggle(false);

            _heroSelectCompleteBus.RunSequential_BusEvents();
            return;
        }
    }
}