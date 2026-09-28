using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMap_Icon : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Image _image;
    public Image image => _image;
    
    [SerializeField] private Animator_Controller _animController;
    public Animator_Controller animController => _animController;


    // Button
    public void Select()
    {
        StageMap_Manager stageMap = GameManager.instance.stageManager.stageMap;
        stageMap.SelectStage_byMapIcon(this);
    }
}