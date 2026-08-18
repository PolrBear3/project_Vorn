using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Stage_ScrObj _newGameStage;


    private StageData _currentData;
    public StageData currentData => _currentData;


    private EventBus_Controller _stageSetEventBus = new();
    public EventBus_Controller stageSetEventBus => _stageSetEventBus;

    private EventBus_Controller _endTurnEventBus = new();
    public EventBus_Controller endTurnEventBus => _endTurnEventBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        GameManager.instance.tileManager.generateEventBus.UnRegister(Set_Stage);
        Input_Controller.instance.OnInteractPressed -= End_Turn;
    }


    // Data
    private void Set_Data()
    {
        GameManager.instance.tileManager.generateEventBus.Register(0, Set_Stage);
        Input_Controller.instance.OnInteractPressed += End_Turn;
    }

    private void Set_Stage(Stage_ScrObj stage)
    {
        _currentData = new(stage);
        StartCoroutine(_stageSetEventBus.RunSequential_DelayBusEvents());
    }

    private void Set_Stage()
    {
        StartCoroutine(RegisterDelay_SetStage());
    }
    private IEnumerator RegisterDelay_SetStage()
    {
        yield return null;
        Set_Stage(_newGameStage);
    }


    // Gameplay
    private void End_Turn(bool isPressed)
    {
        if (isPressed == false) return;

        if (_stageSetEventBus.delayBusRunning) return;
        if (_endTurnEventBus.delayBusRunning) return;
        if (GameManager.instance.cardManager.CardPlace_ActionRunning()) return;

        _endTurnEventBus.RunSequential_BusEvents();
        StartCoroutine(_endTurnEventBus.RunSequential_DelayBusEvents());
    }
}
