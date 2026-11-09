using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Plugins.Animate_UI_Materials;

public class StageMap_Icon : HoverUI
{
    [Space(40)]
    [SerializeField] private RectTransform _playerIconAnchor;
    public RectTransform playerIconAnchor => _playerIconAnchor;

    [SerializeField] private Color _completedStateColor;


    // HoverUI
    public override void OnHover_Update(bool _)
    {

    }

    public override void OnSelect_Update()
    {
        StageMap_Manager stageMap = GameManager.instance.stageManager.stageMap;

        if (stageMap.data.stageSelected) return;
        StartCoroutine(stageMap.SelectStage_byMapIcon(this));
    }


    // StageMap_Icon
    public void Update_CompletedState()
    {
        greyscaleTint.PropertyValue = _completedStateColor;
    }
}