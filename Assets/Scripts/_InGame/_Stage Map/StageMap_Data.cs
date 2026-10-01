using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageMap_Data
{
    [ES3Serializable] private List<List<StageData>> _stagesByLevelDatas = new();
    public List<List<StageData>> stagesByLevelDatas => _stagesByLevelDatas;

    [ES3Serializable] private int _currentLevel;
    public int currentLevel => _currentLevel;

    [ES3Serializable] private int _currentStageIndex;
    public int currentStageIndex => _currentStageIndex;


    // New
    public StageMap_Data(List<List<StageData>> newStagesByLevel)
    {
        if (newStagesByLevel == null || newStagesByLevel.Count <= 0) return;

        _stagesByLevelDatas = new(newStagesByLevel);
        _currentLevel = 0;
    }

    public StageMap_Data(StageMap_Data loadData)
    {
        if (loadData == null) return;

        List<List<StageData>> stagesByLevel = loadData._stagesByLevelDatas;
        if (stagesByLevel == null || stagesByLevel.Count <= 0) return;

        _stagesByLevelDatas = loadData._stagesByLevelDatas;
        _currentLevel = loadData._currentLevel;
        _currentStageIndex = loadData._currentStageIndex;
    }


    // Data
    public List<StageData> StageDatas(bool containEmptyStages)
    {
        List<StageData> allStageDatas = new();

        for (int i = 0; i < _stagesByLevelDatas.Count; i++)
        {
            List<StageData> stageDatas = _stagesByLevelDatas[i];

            foreach (StageData data in stageDatas)
            {
                if (containEmptyStages == false && data == null) continue;
                allStageDatas.Add(data);
            }
        }
        return allStageDatas;
    }
    public List<StageData> TargetLevel_StageDatas(int targetLevel, bool containEmptyStages)
    {
        int levelCount = _stagesByLevelDatas.Count;
        if (levelCount <= 0) return new();

        targetLevel = Mathf.Clamp(targetLevel, 0, levelCount - 1);
        List<StageData> targetLevelStages = new(_stagesByLevelDatas[targetLevel]);

        for (int i = targetLevelStages.Count - 1; i >= 0; i--)
        {
            if (containEmptyStages) continue;
            if (targetLevelStages[i] != null) continue;

            targetLevelStages.RemoveAt(i);
        }
        return targetLevelStages;
    }

    public StageData StageData_byIndex(int stageIndex)
    {
        List<StageData> stageDatas = StageDatas(true);
        if (stageDatas == null || stageDatas.Count <= 0) return null;

        return stageDatas[Mathf.Clamp(stageIndex, 0, stageDatas.Count - 1)];
    }
    public StageData Current_StageData()
    {
        return StageData_byIndex(_currentStageIndex);
    }

    public int TargetStage_CurrentLevel(StageData targetStageData)
    {
        for (int i = 0; i < _stagesByLevelDatas.Count; i++)
        {
            if (_stagesByLevelDatas[i].Contains(targetStageData) == false) continue;
            return i;
        }
        return -1;
    }
    public bool TargetStage_onCurrentLevel(StageData targetStageData)
    {
        int currentLevel = Mathf.Clamp(_currentLevel, 0, _stagesByLevelDatas.Count - 1);
        return _stagesByLevelDatas[currentLevel].Contains(targetStageData);
    }

    public void Update_CurrentStageIndex(int stageIndex)
    {
        _currentStageIndex = Mathf.Clamp(stageIndex, 0, StageDatas(true).Count - 1);
    }
    public void Increase_CurrentLevel()
    {
        _currentLevel = Mathf.Min(_currentLevel + 1, _stagesByLevelDatas.Count - 1);
    }
}