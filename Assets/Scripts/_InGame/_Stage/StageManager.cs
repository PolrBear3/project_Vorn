using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour, ISubscriptionHandler
{
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

        _endStageEventBus.Register(_endStageEventBus.DelayBus_Running);

        _setStageEventBus.OnSequentialDelayFinish += Toggle_BattleStage;
        _endStageEventBus.Register(0, Toggle_BattleStage);

        _setStageEventBus.Register(0, ToggleQueued_EventStage);
        _endTurnEventBus.Register(Is_EventStage);

        Input_Controller.instance.OnInteractPressed += Continue;
    }

    public void UnSubscribe_All()
    {
        _endTurnEventBus.UnRegister(_endTurnEventBus.DelayBus_Running);
        _endTurnEventBus.UnRegister(_setStageEventBus.DelayBus_Running);

        _endStageEventBus.UnRegister(_endStageEventBus.DelayBus_Running);

        _setStageEventBus.OnSequentialDelayFinish += Toggle_BattleStage;
        _endStageEventBus.UnRegister(Toggle_BattleStage);

        _setStageEventBus.UnRegister(ToggleQueued_EventStage);
        _endTurnEventBus.UnRegister(Is_EventStage);

        Input_Controller.instance.OnInteractPressed -= Continue;
    }


    // Battle Stage
    public bool Is_BattleStage()
    {
        StageData currentStageData = _stageMap.data.Current_StageData();
        if (currentStageData == null) return false;

        return currentStageData.stage is BattleStage_ScrObj;
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
    public void Toggle_BattleStage()
    {
        Toggle_BattleStage(Is_BattleStage());
    }


    // Event Stage
    private bool Is_EventStage(out EventStage_ScrObj currentEventStage)
    {
        StageData currentStageData = _stageMap.data.Current_StageData();

        currentEventStage = null;

        if (currentStageData == null) return false;
        if (currentStageData.stage is not EventStage_ScrObj eventStage) return false;

        currentEventStage = eventStage;
        return true;
    }
    public bool Is_EventStage()
    {
        return Is_EventStage(out EventStage_ScrObj _);
    }

    public void ToggleQueued_EventStage()
    {
        if (Is_EventStage() == false) return;

        StageMap_Data stageMapData = _stageMap.data;

        Event_ScrObj queuedEvent = stageMapData.Current_StageData().GetCurrent_QueueEvent();
        GameManager.instance.eventMenuManager.Toggle_EventMenu(queuedEvent);

        if (queuedEvent != null) return;
        
        stageMapData.Complete_CurrentStageData();
        Continue();
    }


    // Set Stage
    private void Load_CurrentStage()
    {
        StartCoroutine(Run_CurrentStageLoad_EventBus());
    }
    private IEnumerator Run_CurrentStageLoad_EventBus()
    {
        yield return null; // wait 1 frame for all events registeration to _stageSetEventBus

        _setStageEventBus.RunSequential_BusEvents();
        StartCoroutine(_setStageEventBus.RunSequential_DelayBusEvents());
    }

    private void Continue()
    {
        StartCoroutine(Run_ContinueEvents());
    }
    private void Continue(bool isPressed)
    {
        if (isPressed == false) return;
        Continue();
    }
    private IEnumerator Run_ContinueEvents()
    {
        _endTurnEventBus.RunSequential_BusEvents();
        StartCoroutine(_endTurnEventBus.RunSequential_DelayBusEvents());

        while (_endTurnEventBus.DelayBus_Running()) yield return null;

        _endStageEventBus.RunSequential_BusEvents();
        StartCoroutine(_endStageEventBus.RunSequential_DelayBusEvents());
    }
}