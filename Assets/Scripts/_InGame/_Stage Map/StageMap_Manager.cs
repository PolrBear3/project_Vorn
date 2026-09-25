using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelSet_StagesData
{
    [SerializeField][Range(0, 3)] private int _maxStageCount;
    [SerializeField] private Stage_ScrObj[] _setStages;

    public int Max_StageCount()
    {
        return Mathf.Max(1, _maxStageCount);
    }

    public List<Stage_ScrObj> Set_Stages()
    {
        List<Stage_ScrObj> setStages = new();

        for (int i = 0; i < _setStages.Length; i++)
        {
            setStages.Add(_setStages[i]);
        }
        return setStages;
    }
}

public class StageMap_Manager : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private StageMap_Icon[] _icons;

    [Space(20)]
    [SerializeField] private LevelSet_StagesData[] _levelSetStageDatas;


    private const int _maxLevel = 9;
    private const int _maxStagePerLevel = 3;

    private StageMap_Data _data; // for save & load
    public StageMap_Data data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_InputData);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_InputData);


        // from Set_Data
        Input_Controller.instance.OnAction1 -= Set_Data;
    }


    // Database
    private List<Stage_ScrObj> TargetLevel_Stages(int targetLevel)
    {
        int levelCount = _levelSetStageDatas.Length;
        if (levelCount <= 0) return null;

        targetLevel = Mathf.Clamp(targetLevel, 0, levelCount - 1);
        return _levelSetStageDatas[targetLevel].Set_Stages();
    }
    private List<Stage_ScrObj> RangeLevel_Stages(int rangeMinLevel, int rangeMaxLevel)
    {
        int levelCount = _levelSetStageDatas.Length;
        if (levelCount <= 0) return null;

        List<Stage_ScrObj> rangeLevelStages = new();

        rangeMinLevel = Mathf.Max(0, rangeMinLevel);
        rangeMaxLevel = Mathf.Min(rangeMaxLevel, levelCount - 1);

        for (int i = rangeMinLevel; i <= rangeMaxLevel; i++)
        {
            List<Stage_ScrObj> targetLevelStages = TargetLevel_Stages(i);

            foreach (Stage_ScrObj stage in targetLevelStages)
            {
                rangeLevelStages.Add(stage);
            }
        }
        return rangeLevelStages;
    }


    // Data
    private List<List<StageData>> NewRun_StageDatas_byLevel()
    {
        int levelCount = _levelSetStageDatas.Length;
        if (levelCount <= 0) return null;

        List<List<StageData>> generatedDatas = new();

        for (int i = 0; i < levelCount; i++)
        {
            LevelSet_StagesData levelSetStagesData = _levelSetStageDatas[i];
            List<StageData> newStageDatas = new();

            List<Stage_ScrObj> availableStages = levelSetStagesData.Set_Stages();

            // effect calculation relative to previous level's stage count []
            int randStageCount = UnityEngine.Random.Range(1, levelSetStagesData.Max_StageCount() + 1);

            for (int j = 0; j < randStageCount; j++)
            {
                if (availableStages.Count <= 0) break;

                // stage type selecting relative to previous level's stages type []
                int randStageIndex = UnityEngine.Random.Range(0, availableStages.Count);
                newStageDatas.Add(new(availableStages[randStageIndex]));

                availableStages.RemoveAt(randStageIndex);
            }
            for (int j = 0; j < _maxStagePerLevel; j++)
            {
                if (newStageDatas.Count >= _maxStagePerLevel) break;

                int randInsertIndex = UnityEngine.Random.Range(0, newStageDatas.Count + 1);
                newStageDatas.Insert(randInsertIndex, null);
            }

            generatedDatas.Add(newStageDatas);
        }
        return generatedDatas;
    }
    private void Set_Data()
    {
        _data = new(NewRun_StageDatas_byLevel());
        Update_MapIcons();
    }

    private void Set_InputData()
    {
        Input_Controller.instance.OnAction1 += Set_Data;
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

            // update stage icon
        }
    }
}
