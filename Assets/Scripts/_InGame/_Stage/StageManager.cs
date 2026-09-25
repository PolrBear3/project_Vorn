using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private StageData _currentData;
    public StageData currentData => _currentData;


    private EventBus_Controller _stageSetEventBus = new();
    public EventBus_Controller stageSetEventBus => _stageSetEventBus;

    private EventBus_Controller _endTurnEventBus = new();
    public EventBus_Controller endTurnEventBus => _endTurnEventBus;

    private EventBus_Controller _stageEndEventBus = new();
    public EventBus_Controller stageEndEventBus => _stageEndEventBus;


    [Space(10)]
    [SerializeField] private GameObject _battleStageContents;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        GameManager.instance.tileManager.generateEventBus.UnRegister(Load_CurrentStage);

        _endTurnEventBus.UnRegister(Is_EventStage);
        _endTurnEventBus.UnRegister(_endTurnEventBus.DelayBus_Running);
        _endTurnEventBus.UnRegister(_stageSetEventBus.DelayBus_Running);

        Input_Controller.instance.OnInteractPressed -= End_Turn;
    }


    // Data
    private void Set_Data()
    {
        GameManager.instance.tileManager.generateEventBus.Register(0, Load_CurrentStage);

        _endTurnEventBus.Register(Is_EventStage);
        _endTurnEventBus.Register(_endTurnEventBus.DelayBus_Running);
        _endTurnEventBus.Register(_stageSetEventBus.DelayBus_Running);

        Input_Controller.instance.OnInteractPressed += End_Turn;
    }

    public bool Is_BattleStage()
    {
        if (_currentData == null) return false;
        return _currentData.stage is BattleStage_ScrObj;
    }
    private bool Is_EventStage()
    {
        return Is_BattleStage() == false;
    }


    // Set Stage
    private void Set_Stage(Stage_ScrObj stage)
    {
        _currentData = new(stage);
        StartCoroutine(Run_StageSetEventBus());
    }
    private IEnumerator Run_StageSetEventBus()
    {
        yield return null; // wait 1 frame for all events registeration to _stageSetEventBus

        _stageSetEventBus.RunSequential_BusEvents();
        StartCoroutine(_stageSetEventBus.RunSequential_DelayBusEvents());
    }

    private void Load_CurrentStage()
    {
        Set_Stage(GameManager.instance.currentGameData.stage);
    }


    // End Turn & Stage
    private void End_Turn(bool isPressed)
    {
        if (isPressed == false) return;
        if (_endTurnEventBus.DelayBus_Running()) return;

        StartCoroutine(Run_EndTurnEventBus());
    }
    private IEnumerator Run_EndTurnEventBus()
    {
        _endTurnEventBus.RunSequential_BusEvents();
        StartCoroutine(_endTurnEventBus.RunSequential_DelayBusEvents());

        while (_endTurnEventBus.DelayBus_Running()) yield return null;

        _stageEndEventBus.RunSequential_BusEvents();
        StartCoroutine(_stageEndEventBus.RunSequential_DelayBusEvents());

        if (_stageEndEventBus.RunCondition_Available() == false) yield break;
        while (_stageEndEventBus.DelayBus_Running()) yield return null;

        _battleStageContents.gameObject.SetActive(false);
    }
}