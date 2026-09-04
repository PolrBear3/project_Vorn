using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Hero_StatPanel : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private RectTransform _rectTransform;
    public RectTransform rectTransform => _rectTransform;

    [SerializeField] private TextMeshProUGUI _valueText;

    [Space(20)]
    [SerializeField] private Image _materialBackground;
    [SerializeField][Range(0, 10)] private float _backgroundEffectSpeed;


    // MonoBehaviour
    private void Awake()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }
    
    private void OnApplicationQuit()
    {
        _materialBackground.material.SetFloat("_Speed", 0f);
    }


    // Visual
    public void Update_ValueText(int currentValue, int maxValue)
    {
        _valueText.text = currentValue + "/" + maxValue;
    }
}
