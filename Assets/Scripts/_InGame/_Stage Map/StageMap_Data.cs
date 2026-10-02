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

    [ES3Serializable] private int _currentLevelStageIndex;
    public int currentLevelStageIndex => _currentLevelStageIndex;

    [ES3Serializable] private bool _stageSelected;
    public bool stageSelected => _stageSelected;


    // New
    public StageMap_Data(List<List<StageData>> newStagesByLevel)
    {
        if (newStagesByLevel == null || newStagesByLevel.Count <= 0) return;

        _stagesByLevelDatas = new(newStagesByLevel);
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

    /// <summary>
    /// Auto updates _currentLevelStageIndex if stage was not selected before data save
    /// </summary>
    public StageData Current_StageData()
    {
        List<StageData> currentLevelStageDatas = TargetLevel_StageDatas(_currentLevel, true);
        if (currentLevelStageDatas == null || currentLevelStageDatas.Count <= 0) return null;
        
        int currentStageIndex = Mathf.Clamp(_currentLevelStageIndex, 0, currentLevelStageDatas.Count - 1);
        StageData currentStageData = currentLevelStageDatas[currentStageIndex];

        if (currentStageData != null) return currentStageData;

        for (int i = 0; i < currentLevelStageDatas.Count; i++)
        {
            currentStageData = currentLevelStageDatas[i];
            if (currentStageData == null) continue;

            _currentLevelStageIndex = i;
            break;
        }
        return currentStageData;
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
    public bool TargetStage_onCurrentLevel(StageData targetStageData, out int levelStageIndex)
    {
        int currentLevel = Mathf.Clamp(_currentLevel, 0, _stagesByLevelDatas.Count - 1);
        List<StageData> currentLevelStageDatas = _stagesByLevelDatas[currentLevel];
        
        for (int i = 0; i < currentLevelStageDatas.Count; i++)
        {
            if (targetStageData != currentLevelStageDatas[i]) continue;

            levelStageIndex = i;
            return true;
        }
        levelStageIndex = 0;
        return false;
    }

    public void Increase_CurrentLevel()
    {
        _currentLevel = Mathf.Min(_currentLevel + 1, _stagesByLevelDatas.Count - 1);
        _stageSelected = false;
    }
    public void Update_CurrentLevel_StageIndex(int stageIndex)
    {
        List<StageData> currentLevelStageDatas = TargetLevel_StageDatas(_currentLevel, true);
        if (currentLevelStageDatas == null) return;

        _currentLevelStageIndex = Mathf.Clamp(stageIndex, 0, currentLevelStageDatas.Count - 1);
        _stageSelected = true;
    }
}