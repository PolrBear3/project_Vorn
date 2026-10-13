using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelect_Manager : MonoBehaviour, ISubscriptionHandler
{
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

    }

    public void UnSubscribe_All()
    {

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

    private void Toggle_HeroSelect()
    {
        HeroManager_Data data = GameManager.instance.heroManager.data;
        if (data.selectedHero != null) return;

        List<Hero_ScrObj> selectableHeroes = new(data.selectableHeros);
        for (int i = 0; i < _selectGroups.Length; i++)
        {
            Hero_SelectGroup selectGroup = _selectGroups[i];

            selectGroup.Update_SelectHero(selectableHeroes[i]);
            selectGroup.Update_Coins(0);
        }
        _toggleController.Toggle(true);
    }
    public void Select_Hero(Hero_SelectGroup selectGroup)
    {
        HeroManager_Data data = GameManager.instance.heroManager.data;

        for (int i = 0; i < _selectGroups.Length; i++)
        {
            if (selectGroup != _selectGroups[i]) continue;

            data.Update_SelectedHero(data.selectableHeros[i]);

            _toggleController.Toggle(false);
            _heroSelectCompleteBus.RunSequential_BusEvents();

            return;
        }
    }
}