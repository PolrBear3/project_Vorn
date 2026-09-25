using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StageCount_Probability
{
    [SerializeField][Range(0, 100)] private int[] _stageCountProbability;

    public int WeightRandom_StageCount()
    {
        if (_stageCountProbability.Length <= 0) return 0;
        
        int totalWeight = 0;
        for (int i = 0; i < _stageCountProbability.Length; i++)
        {
            totalWeight += _stageCountProbability[i];
        }
        if (totalWeight <= 0) return 0;

        int randWeight = Random.Range(0, totalWeight);
        for (int i = 0; i < _stageCountProbability.Length; i++)
        {
            if (randWeight < _stageCountProbability[i]) return i;
            randWeight -= _stageCountProbability[i];
        }
        return 0;
    }
}

[System.Serializable]
public class LevelSet_StagesData
{
    [SerializeField] private Stage_ScrObj[] _setStages;
    [SerializeField][Range(0, 3)]  private int _fixedMaxStageCount;

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

public class StageMap_Manager : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private StageMap_Icon[] _icons;

    [Space(20)]
    [SerializeField] private LevelSet_StagesData[] _levelSetStageDatas;

    [Space(20)]
    [SerializeField] private StageCount_Probability[] _ifPreviousStageCountIs; // index + 1 is previous level stage count
    [SerializeField] private StageCount_Probability[] _ifPreviousEventCountIs; // index is previous event stage count


    private const int _maxLevel = 9;
    private const int _maxStagePerLevel = 3;

    private StageMap_Data _data; // for save & load
    public StageMap_Data data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Subscriptions);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Subscriptions);


        // from Set_Subscriptions
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageEndEventBus.UnRegister(ToggleMenu);
        stageManager.endTurnEventBus.UnRegister(MenuToggled);


        Input_Controller.instance.OnAction1 -= Set_Data;
    }


    // Data
    private int StageCount(List<StageData> targetDatas)
    {
        int count = 0;

        for (int i = 0; i < targetDatas.Count; i++)
        {
            if (targetDatas[i] == null) continue;
            count ++;
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
            count ++;
        }
        return count;
    }
    
    private List<StageData> NewRun_StageDatas(LevelSet_StagesData levelStageData, int totalStageCount, int eventStageCount)
    {
        List<StageData> newRunDatas = new();

        List<Stage_ScrObj> eventStages = levelStageData.SetAvailable_Stages(StageType.Event);

        for (int i = 0; i < eventStageCount; i++) // event stage
        {
            if (eventStages.Count <= 0) break;
            int randStageIndex = UnityEngine.Random.Range(0, eventStages.Count);

            newRunDatas.Add(new(eventStages[randStageIndex]));
            eventStages.RemoveAt(randStageIndex);
        }

        List<Stage_ScrObj> battleStages = levelStageData.SetAvailable_Stages(StageType.Battle);
        int battleStageCount = totalStageCount - newRunDatas.Count;

        for (int i = 0; i < battleStageCount; i++) // battle stage
        {
            if (battleStages.Count <= 0) break;
            int randStageIndex = UnityEngine.Random.Range(0, battleStages.Count);

            newRunDatas.Add(new(battleStages[randStageIndex]));
            battleStages.RemoveAt(randStageIndex);
        }

        if (newRunDatas.Count <= 0) newRunDatas.Add(new(levelStageData.Random_SetAvailableStage()));

        for (int j = 0; j < _maxStagePerLevel; j++) // empty stage fill
        {
            if (newRunDatas.Count >= _maxStagePerLevel) break;
            newRunDatas.Add(null);
        }

        for (int i = 0; i < newRunDatas.Count; i++) // shuffle
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
            previousStageCount = StageCount(newStageDatas);

            generatedDatas.Add(newStageDatas); // add to level
        }
        return generatedDatas;
    }
    
    private void Set_Data()
    {
        _data = new(NewRun_StageDatas_byLevel());
        Update_MapIcons();
    }
    private void Set_Subscriptions()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.stageEndEventBus.Register(0, ToggleMenu);
        stageManager.endTurnEventBus.Register(MenuToggled);


        Input_Controller.instance.OnAction1 += Set_Data;
    }


    // Menu
    private void ToggleMenu(bool toggle)
    {
        _menuPanel.SetActive(toggle);

        if (toggle == false) return;
        Update_MapIcons();
    }
    private void ToggleMenu()
    {
        ToggleMenu(true);
    }

    private bool MenuToggled()
    {
        return _menuPanel.activeSelf;
    }


    // StageMap Icon
    private void Update_MapIcons()
    {
        if (_data == null) return;

        List<StageData> updateDatas = _data.StageDatas();
        if (updateDatas == null || updateDatas.Count <= 0) return;

        for (int i = 0; i < _icons.Length; i++)
        {
            StageData dataToUpdate = i < updateDatas.Count ? updateDatas[i] : null;

            bool stageAvailable = dataToUpdate != null & dataToUpdate?.stage != null;
            _icons[i].image.color = stageAvailable ? Color.white : Color.clear;

            if (stageAvailable == false) continue;
            _icons[i].image.sprite = dataToUpdate.stage.stageIcon;
        }
    }
}
