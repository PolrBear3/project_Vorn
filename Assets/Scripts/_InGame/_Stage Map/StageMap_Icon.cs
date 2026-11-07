using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMap_Icon : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _image;
    public Image image => _image;

    [SerializeField] private Animator_Controller _animController;
    public Animator_Controller animController => _animController;

    [Space(20)]
    [SerializeField] private RectTransform _playerIconAnchor;
    public RectTransform playerIconAnchor => _playerIconAnchor;


    // Button
    public void Select()
    {
        StageMap_Manager stageMap = GameManager.instance.stageManager.stageMap;

        if (stageMap.data.stageSelected) return;
        StartCoroutine(stageMap.SelectStage_byMapIcon(this));
    }
}