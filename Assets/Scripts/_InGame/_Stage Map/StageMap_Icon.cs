using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMap_Icon : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Image _image;
    public Image image => _image;

    [SerializeField] private Animator _animator;


    private StageData _data;
    public StageData data => _data;
}
