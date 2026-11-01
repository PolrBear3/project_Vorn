using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionBar : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Transform _bar;
    [SerializeField] private Transform _fill;

    [Space(20)]
    [SerializeField] private SpriteRenderer _valueBox;
    [SerializeField] private TextMeshPro _valueText;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _sidePaddingValue;


    private float _barTogglePosX;


    // MonoBehaviour
    private void Awake()
    {
        _barTogglePosX = _bar.localPosition.x;
    }


    // UI
    public void Update_Fill(int maxValue, int currentValue)
    {
        float fillRatio = Mathf.Clamp01((float)currentValue / maxValue);

        Vector3 fillScale = _fill.localScale;
        fillScale.x = fillRatio;

        _fill.localScale = fillScale;
        _valueText.text = currentValue + "/" + maxValue;
    }

    public void Toggle_ValueBox(bool toggle)
    {
        float xPos = toggle ? _barTogglePosX : 0f;

        _bar.transform.localPosition = new(xPos, 0f);
        _valueBox.gameObject.SetActive(toggle);

        if (toggle == false) return;

        float textWidth = _valueText.GetPreferredValues(_valueText.text).x;
        float pixelSize = Utility.worldSpacePixelValue;

        Vector2 textBoxSize = _valueBox.size;
        textBoxSize.x = (Mathf.Ceil(textWidth / pixelSize) + _sidePaddingValue) * pixelSize;

        _valueBox.size = textBoxSize;
    }
}
