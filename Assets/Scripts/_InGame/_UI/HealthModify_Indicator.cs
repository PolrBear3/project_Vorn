using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthModify_Indicator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Sprite[] _microIconSprites;
    [SerializeField] private Sprite[] _toggleIconSprites;

    [Space(20)]
    [SerializeField] private SpriteRenderer _toggleIcon;
    [SerializeField][Range(0, 10)] private int _toggleSpacingValue;

    [Space(20)]
    [SerializeField] private SpriteRenderer _valueBox;
    [SerializeField] private TextMeshPro _valueText;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _sidePaddingValue;


    private Vector2 _defaultTogglePosition;
    private int _recentModifyValue;


    // MonoBehaviour
    private void Awake()
    {
        _defaultTogglePosition = _toggleIcon.transform.localPosition;
    }


    // UI
    public void Update_ModifyValue(int modifyValue)
    {
        _recentModifyValue = modifyValue;
    }

    public void Toggle_ValueBox(bool toggle)
    {
        if (_toggleIcon == null || _valueBox == null) return;

        bool hasModifyValue = _recentModifyValue != 0;

        _toggleIcon.gameObject.SetActive(hasModifyValue);
        _valueBox.gameObject.SetActive(toggle && hasModifyValue);

        if (hasModifyValue == false) return;

        Sprite[] toggleSprites = toggle ? _toggleIconSprites : _microIconSprites;
        int spriteIndex = _recentModifyValue < 0 ? 0 : 1;

        _toggleIcon.sprite = toggleSprites[spriteIndex];
        _toggleIcon.transform.localPosition = toggle ? new(Utility.worldSpacePixelValue * -_toggleSpacingValue, 0f) : _defaultTogglePosition;

        if (toggle == false) return;

        _valueText.text = _recentModifyValue >= 0 ? "+" + _recentModifyValue : _recentModifyValue.ToString();

        float textWidth = _valueText.GetPreferredValues(_valueText.text).x;
        float pixelSize = Utility.worldSpacePixelValue;

        Vector2 textBoxSize = _valueBox.size;
        textBoxSize.x = (Mathf.Ceil(textWidth / pixelSize) + _sidePaddingValue) * pixelSize;

        _valueBox.size = textBoxSize;
    }
}
