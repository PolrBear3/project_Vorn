using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class StageCount_Probability
{
    [SerializeField][Range(0, 100)] private int[] _stageCountWeights;

    public int WeightRandom_StageCount()
    {
        if (_stageCountWeights.Length <= 0) return 0;

        int totalWeight = 0;
        for (int i = 0; i < _stageCountWeights.Length; i++)
        {
            totalWeight += _stageCountWeights[i];
        }
        if (totalWeight <= 0) return 0;

        int randWeight = Random.Range(0, totalWeight);
        for (int i = 0; i < _stageCountWeights.Length; i++)
        {
            if (randWeight < _stageCountWeights[i]) return i;
            randWeight -= _stageCountWeights[i];
        }
        return 0;
    }
}

[System.Serializable]
public class LevelSet_StagesData
{
    [SerializeField] private Stage_ScrObj[] _setStages;
    [SerializeField][Range(0, 3)] private int _fixedMaxStageCount;

    public int Fixed_MaxStageCount()
    {
        return Mathf.Max(1, _fixedMaxStageCount);
    }

    public List<Stage_ScrObj> SetAvailable_Stages()
    {
        List<Stage_ScrObj> setStages = new();

        for (int i = 0; i < _setStages.Length; i++)
        {
            setStages.Add(_setStages[i]);
        }
        return setStages;
    }
    public List<Stage_ScrObj> SetAvailable_Stages(StageType stageType)
    {
        List<Stage_ScrObj> setStages = new();

        for (int i = 0; i < _setStages.Length; i++)
        {
            Stage_ScrObj stage = _setStages[i];

            if (stage.stageType != stageType) continue;
            setStages.Add(_setStages[i]);
        }
        return setStages;
    }

    public Stage_ScrObj Random_SetAvailableStage()
    {
        int setStageCount = _setStages.Length;
        if (setStageCount <= 0) return null;

        return _setStages[UnityEngine.Random.Range(0, setStageCount)];
    }
}

public class StageMap_Manager : MonoBehaviour, ISaveLoadable, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _menuPanelController;
    [SerializeField] private StageMap_Icon[] _icons;

    [Space(20)]
    [SerializeField] private LevelSet_StagesData[] _levelSetStageDatas;

    [SerializeField] private StageCount_Probability[] _ifPreviousStageCountIs; // index + 1 is previous level stage count
    [SerializeField] private StageCount_Probability[] _ifPreviousEventCountIs; // index is previous event stage count


    private const int _maxLevel = 9;
    private const int _maxStagePerLevel = 3;

    private StageMap_Data _data; // for save & load
    public StageMap_Data data => _data;


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
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageEndEventBus.Register(0, Complete_CurrentStage);
        stageManager.stageEndEventBus.Register(0, ToggleMenu);
        stageManager.endTurnEventBus.Register(MenuToggled);

        Input_Controller.instance.OnAction1 += Set_NewData;
    }

    public void UnSubscribe_All()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageEndEventBus.UnRegister(Complete_CurrentStage);
        stageManager.stageEndEventBus.UnRegister(ToggleMenu);
        stageManager.endTurnEventBus.UnRegister(MenuToggled);

        Input_Controller.instance.OnAction1 -= Set_NewData;
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("StageMap_Manager/StageMap_Data", _data);
    }

    public void Load_Data()
    {
        if (ES3.KeyExists("StageMap_Manager/StageMap_Data") == false)
        {
            Set_NewData();
            return;
        }
        _data = ES3.Load<StageMap_Data>("StageMap_Manager/StageMap_Data");
    }


    // Data
    private int Available_StageCount(List<StageData> targetDatas)
    {
        int count = 0;

        for (int i = 0; i < targetDatas.Count; i++)
        {
            if (targetDatas[i] == null) continue;
            count++;
        }
        return count;
    }
    private int StageType_Count(List<StageData> targetDatas, StageType targetType)
    {
        int count = 0;

        for (int i = 0; i < targetDatas.Count; i++)
        {
            StageData data = targetDatas[i];
            if (data == null) continue;

            if (targetType != targetDatas[i].stage.stageType) continue;
            count++;
        }
        return count;
    }

    private void Complete_CurrentStage()
    {
        if (_data == null) return;

        Hero currentHero = GameManager.instance.heroManager.currentHero;
        if (currentHero == null) return;

        if (currentHero.data.currentData.currentHealth <= 0) return;

        _data.Increase_CurrentLevel();
        _data.Current_StageData().Toggle_CompleteState(true);
    }


    private void AddRandomStages_toTargetData(List<StageData> targetData, List<Stage_ScrObj> addStages, int addAmount)
    {
        if (targetData == null || addStages == null) return;

        for (int i = 0; i < addAmount; i++)
        {
            if (addStages.Count <= 0) return;
            int randStageIndex = UnityEngine.Random.Range(0, addStages.Count);

            targetData.Add(new(addStages[randStageIndex]));
            addStages.RemoveAt(randStageIndex);
        }
    }
    private List<StageData> NewRun_StageDatas(LevelSet_StagesData newLevelStageDatas, int totalStageCount, int eventStageCount)
    {
        List<StageData> newRunDatas = new();

        List<Stage_ScrObj> eventStages = newLevelStageDatas.SetAvailable_Stages(StageType.Event);
        AddRandomStages_toTargetData(newRunDatas, eventStages, eventStageCount);

        List<Stage_ScrObj> battleStages = newLevelStageDatas.SetAvailable_Stages(StageType.Battle);
        AddRandomStages_toTargetData(newRunDatas, battleStages, totalStageCount - newRunDatas.Count);

        // duplicate stage fill
        List<Stage_ScrObj> availableStages = newLevelStageDatas.SetAvailable_Stages();
        AddRandomStages_toTargetData(newRunDatas, availableStages, totalStageCount - newRunDatas.Count);

        // empty stage fill
        for (int i = 0; i < _maxStagePerLevel; i++)
        {
            if (newRunDatas.Count >= _maxStagePerLevel) break;
            newRunDatas.Add(null);
        }

        // shuffle
        for (int i = 0; i < newRunDatas.Count; i++)
        {
            StageData tempData = newRunDatas[i];
            int randIndex = UnityEngine.Random.Range(i, newRunDatas.Count);

            newRunDatas[i] = newRunDatas[randIndex];
            newRunDatas[randIndex] = tempData;
        }
        return newRunDatas;
    }

    private List<List<StageData>> NewRun_StageDatas_byLevel()
    {
        int levelCount = _levelSetStageDatas.Length;
        if (levelCount <= 0) return null;

        List<List<StageData>> generatedDatas = new();
        int previousStageCount = 1;

        for (int i = 0; i < levelCount; i++)
        {
            LevelSet_StagesData levelSetStagesData = _levelSetStageDatas[i];

            int randStageCount = _ifPreviousStageCountIs[previousStageCount - 1].WeightRandom_StageCount() + 1; // index + 1 level stage count
            randStageCount = Mathf.Clamp(randStageCount, 1, levelSetStagesData.Fixed_MaxStageCount()); // set available stage count

            int previousLevelEventCount = generatedDatas.Count > 0 ? StageType_Count(generatedDatas[i - 1], StageType.Event) : 0;

            int randEventStageCount = _ifPreviousEventCountIs[previousLevelEventCount].WeightRandom_StageCount(); // set event stage count
            randEventStageCount = Mathf.Min(randEventStageCount, randStageCount);

            List<StageData> newStageDatas = NewRun_StageDatas(levelSetStagesData, randStageCount, randEventStageCount); // combine set stages
            previousStageCount = Available_StageCount(newStageDatas);

            generatedDatas.Add(newStageDatas); // add to level
        }
        return generatedDatas;
    }
    private void Set_NewData()
    {
        _data = new(NewRun_StageDatas_byLevel());

        List<StageData> startingStageDatas = _data.TargetLevel_StageDatas(0, false);
        StageData startingStageData = startingStageDatas[UnityEngine.Random.Range(0, startingStageDatas.Count)];

        List<StageData> allStageDatas = _data.StageDatas(true);
        for (int i = 0; i < allStageDatas.Count; i++)
        {
            if (startingStageData != allStageDatas[i]) continue;

            _data.Update_CurrentStageIndex(i);
            return;
        }
    }


    // Menu
    private void ToggleMenu(bool toggle)
    {
        _menuPanelController.Toggle(toggle);

        if (toggle == false) return;
        Update_MapIconVisuals();
    }
    private void ToggleMenu()
    {
        ToggleMenu(true);
    }

    private bool MenuToggled()
    {
        return _menuPanelController.toggled;
    }


    // Stage Map Icon
    private void Update_MapIconVisuals()
    {
        if (_data == null) return;

        List<StageData> updateDatas = _data.StageDatas(true);
        if (updateDatas == null || updateDatas.Count <= 0) return;

        for (int i = 0; i < _icons.Length; i++)
        {
            StageMap_Icon icon = _icons[i];
            StageData dataToUpdate = i < updateDatas.Count ? updateDatas[i] : null;

            bool stageAvailable = dataToUpdate != null & dataToUpdate?.stage != null;
            icon.image.color = stageAvailable ? Color.white : Color.clear;

            if (stageAvailable == false) continue;

            icon.image.sprite = dataToUpdate.stage.stageIcon;
            Animator_Controller animController = icon.animController;

            if (_data.TargetStage_onCurrentLevel(dataToUpdate))
            {
                animController.Play_State(UIAnimation.Available); // white
                continue;
            }
            if (dataToUpdate.completed || _data.TargetStage_CurrentLevel(dataToUpdate) > _data.currentLevel)
            {
                animController.Play_State(UIAnimation.Toggle); // brown
                continue;
            }
            animController.Play_State(UIAnimation.Restricted); // grey
        }
    }
    public void SelectStage_byMapIcon(StageMap_Icon selectedIcon)
    {
        if (_data == null) return;
        List<StageData> currentStageDatas = _data.StageDatas(true);

        for (int i = 0; i < _icons.Length; i++)
        {
            if (selectedIcon != _icons[i]) continue;

            StageData selectedData = currentStageDatas[i];
            if (selectedData == null) return;

            _data.Update_CurrentStageIndex(i);
            Save_Data();

            EventBus_GlobalController.UnSubscribeAll_SubscriptionHandlers();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            return;
        }
    }
}
