using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType { Battle, Event }

public abstract class Stage_ScrObj : ScriptableObject
{
    public abstract StageType stageType { get; }


    [Space(10)]
    [SerializeField] private Sprite _stageIcon;
    public Sprite stageIcon => _stageIcon;

    [SerializeField][TextArea(3, 10)] private string _stageDescription;
    public string stageDescription => _stageDescription;
}