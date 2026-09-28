using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageMap_Data
{
    [SerializeField] private List<List<StageData>> _stagesByLevelDatas = new();
    public List<List<StageData>> stagesByLevelDatas => _stagesByLevelDatas;

    private int _currentLevel;
    public int currentLevel => _currentLevel;


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
    }


    // Data
    public List<StageData> StageDatas()
    {
        List<StageData> allStageDatas = new();
        for (int i = 0; i < _stagesByLevelDatas.Count; i++)
        {
            List<StageData> stageDatas = _stagesByLevelDatas[i];
            foreach (StageData data in stageDatas)
            {
                allStageDatas.Add(data);
            }
        }
        return allStageDatas;
    }
    public List<StageData> TargetLevel_StageDatas(int targetLevel)
    {
        int levelCount = _stagesByLevelDatas.Count;
        if (levelCount <= 0) return new();

        targetLevel = Mathf.Clamp(targetLevel, 0, levelCount - 1);
        return _stagesByLevelDatas[targetLevel];
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

    public void Increase_CurrentLevel()
    {
        _currentLevel = Mathf.Min(_currentLevel + 1, _stagesByLevelDatas.Count);
    }
}