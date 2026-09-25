using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType { Battle, Event }

public abstract class Stage_ScrObj : ScriptableObject
{
    [Space(10)]
    [SerializeField] private StageType _stageType;
    public StageType stageType => _stageType;

    [SerializeField] private Sprite _stageIcon;
    public Sprite stageIcon => _stageIcon;

    [SerializeField][TextArea(3, 10)] private string _stageDescription;
    public string stageDescription => _stageDescription;
}