using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Space(20)]
    [SerializeField] private StageMap_Icon[] _icons;
    [SerializeField] private Sprite _eventStageIconSprite;

    [Space(20)]
    [SerializeField] private Image _playerIcon;
    [SerializeField][Range(0, 10)] private float _playerIconPositionUpdateDuration;

    [Space(20)]
    [SerializeField] private LevelSet_StagesData[] _levelSetStageDatas;

    [SerializeField] private StageCount_Probability[] _ifPreviousStageCountIs; // index + 1 is previous level stage count
    [SerializeField] private StageCount_Probability[] _ifPreviousEventCountIs; // index is previous event stage count


    private const int _maxStagePerLevel = 3;

    private StageMap_Data _data;
    public StageMap_Data data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.StartLoad, ToggleMenu);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);

        EventBus_GlobalController.UnRegister(ToggleMenu);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.endStageEventBus.Register(0, ToggleMenu_OnStageEnd);
        stageManager.setStageEventBus.Register(Stage_NotSelected);
    }

    public void UnSubscribe_All()
    {
        StageManager stageManager = GameManager.instance.stageManager;

        stageManager.endStageEventBus.UnRegister(ToggleMenu_OnStageEnd);
        stageManager.setStageEventBus.UnRegister(Stage_NotSelected);
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("StageMap_Manager/StageMap_Data", _data, SaveKeys.GamePlayFile);
    }

    public void Load_Data()
    {
        if (ES3.KeyExists("StageMap_Manager/StageMap_Data", SaveKeys.GamePlayFile) == false)
        {
            Set_NewData();
            return;
        }
        _data = ES3.Load<StageMap_Data>("StageMap_Manager/StageMap_Data", SaveKeys.GamePlayFile);
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

        StageData startingStage = startingStageDatas[UnityEngine.Random.Range(0, startingStageDatas.Count)];
        if (_data.TargetStage_onCurrentLevel(startingStage, out int levelStageIndex) == false) return;

        _data.Update_CurrentLevel_StageIndex(levelStageIndex);
    }


    // Menu
    private bool Stage_NotSelected()
    {
        return _data.stageSelected == false;
    }

    public void ToggleMenu()
    {
        bool stageDataToggle = Stage_NotSelected() && _data.FinalLevel_Complete() == false;
        bool toggle = stageDataToggle && GameManager.instance.deckBuildManager.data.deckBuildComplete;

        _menuPanelController.Toggle(toggle);
        if (toggle == false) return;

        Update_MapIconVisuals();
        Update_PlayerIcon_Visual();
        Update_PlayerIcon_Position();
    }
    private IEnumerator ToggleMenu_OnStageEnd()
    {
        bool toggle = Stage_NotSelected() && _data.FinalLevel_Complete() == false;
        if (toggle == false) yield break;

        Update_MapIconVisuals();
        Update_PlayerIcon_Visual();
        Update_PlayerIcon_Position();

        yield return _menuPanelController.DelayToggle(toggle);
    }


    // Stage Map Icon
    private bool Target_StageData_IconIndex(StageData targetData, out int iconIndex)
    {
        iconIndex = 0;

        List<StageData> updateDatas = _data.StageDatas(true);
        if (updateDatas == null || updateDatas.Count <= 0) return false;

        for (int i = 0; i < updateDatas.Count; i++)
        {
            if (targetData != updateDatas[i]) continue;

            iconIndex = i;
            return true;
        }
        return false;
    }

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

            Stage_ScrObj stage = dataToUpdate.stage;

            Sprite iconSprite = stage is BattleStage_ScrObj battleStage ? battleStage.stageIcon : _eventStageIconSprite;
            icon.image.sprite = iconSprite;

            Animator_Controller animController = icon.animController;

            if (_data.TargetStage_onCurrentLevel(dataToUpdate, out int _))
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
    public IEnumerator SelectStage_byMapIcon(StageMap_Icon selectedIcon)
    {
        if (_data == null || _data.stageSelected) yield break;

        List<StageData> currentStageDatas = _data.StageDatas(true);

        for (int i = 0; i < _icons.Length; i++)
        {
            if (selectedIcon != _icons[i]) continue;

            StageData selectedData = currentStageDatas[i];

            if (selectedData == null) yield break;
            if (_data.TargetStage_onCurrentLevel(selectedData, out int levelStageIndex) == false) yield break;

            _data.Update_CurrentLevel_StageIndex(levelStageIndex);
            selectedData.Update_QueueEvents();

            yield return Update_PlayerIcon_Position(selectedData);
            GameManager.instance.Continue_GamePlay();

            yield break;
        }
    }


    // Player Icon
    private void Update_PlayerIcon_Visual()
    {
        Hero_ScrObj selectedHero = GameManager.instance.heroManager.data.selectedHero;
        if (selectedHero == null) return;

        _playerIcon.sprite = selectedHero.stageMapIconSprite;
    }

    private void Update_PlayerIcon_Position()
    {
        List<StageData> previousLevelDatas = _data.TargetLevel_StageDatas(_data.currentLevel - 1, true);

        for (int i = 0; i < previousLevelDatas.Count; i++)
        {
            StageData previousData = previousLevelDatas[i];

            if (previousData == null || previousData.completed == false) continue;
            if (Target_StageData_IconIndex(previousData, out int previousIconIndex) == false) continue;

            _playerIcon.transform.SetParent(_icons[previousIconIndex].playerIconAnchor);
            _playerIcon.rectTransform.anchoredPosition = Vector2.zero;

            return;
        }
    }
    private IEnumerator Update_PlayerIcon_Position(StageData updatedStageData)
    {
        _playerIcon.transform.SetParent(_menuPanelController.togglePanelRect);

        if (Target_StageData_IconIndex(updatedStageData, out int iconIndex) == false) yield break;
        StageMap_Icon destinationIcon = _icons[iconIndex];

        LeanTween.move(_playerIcon.gameObject, destinationIcon.playerIconAnchor.position, _playerIconPositionUpdateDuration);
        while (LeanTween.isTweening(_playerIcon.rectTransform)) yield return null;
    }
}
