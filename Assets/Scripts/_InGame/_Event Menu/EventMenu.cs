using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EventMenu : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private UIPanel_ToggleController _toggleController;
    public UIPanel_ToggleController toggleController => _toggleController;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI _eventTitleText;
    [SerializeField] private TextMeshProUGUI _eventDescriptionText;


    private Event_ScrObj _targetEvent;
    public Event_ScrObj targetEvent => _targetEvent;

    public Func<bool> OnConfirmAvailable;


    // Main
    public abstract bool Update_TargetEvent(Event_ScrObj updateEvent);
    public bool ToggleUpdate_TargetEvent(Event_ScrObj updateEvent)
    {
        _targetEvent = updateEvent;

        if (updateEvent == null) return false;
        if (Update_TargetEvent(updateEvent) == false) return false;

        StageData eventStage = GameManager.instance.stageManager.stageMap.data.Current_StageData();

        if (_eventTitleText == null || _eventDescriptionText == null) return true;

        _eventTitleText.text = eventStage != null ? eventStage.stage.stageName : "";
        _eventDescriptionText.text = updateEvent.eventDescription;

        return true;
    }

    public abstract void AutoUpdate_forConfirm();
    public abstract void Confirm();
}