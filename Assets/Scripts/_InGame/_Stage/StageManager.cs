using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class StageManager : MonoBehaviour, ISubscriptionHandler
{
    private StageData _currentData;
    public StageData currentData => _currentData;


    private EventBus_Controller _setStageEventBus = new();
    public EventBus_Controller setStageEventBus => _setStageEventBus;

    private EventBus_Controller _endTurnEventBus = new();
    public EventBus_Controller endTurnEventBus => _endTurnEventBus;

    private EventBus_Controller _endStageEventBus = new();
    public EventBus_Controller endStageEventBus => _endStageEventBus;


    [Space(20)]
    [SerializeField] private StageMap_Manager _stageMap;
    public StageMap_Manager stageMap => _stageMap;

    [Space(20)]
    [SerializeField] private UIPanel_ToggleController[] _battleStagePanelToggles;
    [SerializeField] private Component_ToggleController[] _battleStageComponentToggles;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.AwakeLoad, Load_CurrentStage);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Load_CurrentStage);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        _endTurnEventBus.Register(_endTurnEventBus.DelayBus_Running);
        _endTurnEventBus.Register(_setStageEventBus.DelayBus_Running);

        _setStageEventBus.OnSequentialDelayFinish += Toggle_BattleStage;

        _setStageEventBus.Register(0, Toggle_EventStage);
        _endTurnEventBus.Register(Is_EventStage);

        Input_Controller.instance.OnInteractPressed += End_Turn;
    }
    
    public void UnSubscribe_All()
    {
        _endTurnEventBus.UnRegister(_endTurnEventBus.DelayBus_Running);
        _endTurnEventBus.UnRegister(_setStageEventBus.DelayBus_Running);

        _setStageEventBus.OnSequentialDelayFinish += Toggle_BattleStage;

        _setStageEventBus.UnRegister(Toggle_EventStage);
        _endTurnEventBus.UnRegister(Is_EventStage);

        Input_Controller.instance.OnInteractPressed -= End_Turn;
    }


    // Battle Stage
    public bool Is_BattleStage()
    {
        if (_currentData == null) return false;
        return _currentData.stage is BattleStage_ScrObj;
    }

    private void Toggle_BattleStage(bool toggle)
    {
        for (int i = 0; i < _battleStagePanelToggles.Length; i++)
        {
            _battleStagePanelToggles[i].Toggle(toggle);
        }
        for (int i = 0; i < _battleStageComponentToggles.Length; i++)
        {
            _battleStageComponentToggles[i].Toggle(toggle);
        }
    }
    private void Toggle_BattleStage()
    {
        Toggle_BattleStage(Is_BattleStage());
    }


    // Event Stage
    private bool Is_EventStage(out EventStage_ScrObj currentEventStage)
    {
        currentEventStage = null;

        if (_currentData == null) return false;
        if (_currentData.stage is not EventStage_ScrObj eventStage) return false;

        currentEventStage = eventStage;
        return true;
    }
    private bool Is_EventStage()
    {
        return Is_EventStage(out EventStage_ScrObj _);
    }

    private void Toggle_EventStage()
    {
        if (_currentData == null) return;
        if (Is_EventStage() == false) return;

        EventMenu_Manager eventMenuManager = GameManager.instance.eventMenuManager;
        List<Event_ScrObj> queuedEvents = new(_stageMap.data.Current_StageData().queueEvents);

        if (queuedEvents.Count <= 0)
        {
            return;
        }
        Event_ScrObj queuedEvent = queuedEvents[0];
        eventMenuManager.Toggle_EventMenu(queuedEvent);
    }


    // Set Stage
    private void Load_CurrentStage()
    {
        Set_Stage(_stageMap.data.Current_StageData().stage);
    }

    private void Set_Stage(Stage_ScrObj setStage)
    {
        if (setStage == null) return;
        
        _currentData = new(setStage);
        StartCoroutine(Run_SetStage_EventBus());
    }
    private IEnumerator Run_SetStage_EventBus()
    {
        yield return null; // wait 1 frame for all events registeration to _stageSetEventBus

        _setStageEventBus.RunSequential_BusEvents();
        StartCoroutine(_setStageEventBus.RunSequential_DelayBusEvents());
    }

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

        if (_endStageEventBus.RunCondition_Available() == false) yield break;
        Toggle_BattleStage(false);

        _endStageEventBus.RunSequential_BusEvents();
        StartCoroutine(_endStageEventBus.RunSequential_DelayBusEvents());
    }
}