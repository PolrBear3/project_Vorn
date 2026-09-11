using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ToolTip : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Image _panel;
    public Image panel => _panel;

    [SerializeField] private Vector2 _flipSeperationDistanceX; // x left, y right

    [Space(20)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _iconImage;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Space(20)]
    [SerializeField][Range(0, 10)] private float _toggleDelayTime;


    private Sprite _defaultBaseSprite;
    private Coroutine _toggleDelayCoroutine;

    private List<Func<bool>> _toggleRestrictionChecks = new();
    public List<Func<bool>> toggleRestrictionChecks => _toggleRestrictionChecks;


    // MonoBehaviour
    private void Awake()
    {
        _defaultBaseSprite = _baseImage.sprite;

        Toggle(false);
    }


    // Main
    public void Update_Contents(Sprite baseSprite, Sprite iconSprite, string nameString, string descriptionString)
    {
        _baseImage.sprite = baseSprite != null ? baseSprite : _defaultBaseSprite;
        _iconImage.sprite = iconSprite;

        _nameText.text = nameString;
        _descriptionText.text = descriptionString;
    }

    private bool Toggle_Restricted()
    {
        for (int i = 0; i < _toggleRestrictionChecks.Count; i++)
        {
            if (_toggleRestrictionChecks[i].Invoke() == false) continue;
            return true;
        }
        return false;
    }

    public void Toggle(bool toggle)
    {
        if (_toggleDelayCoroutine != null)
        {
            StopCoroutine(_toggleDelayCoroutine);
            _toggleDelayCoroutine = null;
        }
        if (toggle == false || Toggle_Restricted())
        {
            _panel.gameObject.SetActive(false);
            return;
        }
        _toggleDelayCoroutine = StartCoroutine(DelayToggle());
    }
    private IEnumerator DelayToggle()
    {
        yield return new WaitForSeconds(_toggleDelayTime);
        _panel.gameObject.SetActive(true);

        _toggleDelayCoroutine = null;
    }


    // Cursor
    private void Update_yPivotState()
    {
        RectTransform cursorPointer = GameManager.instance.cursor.pointerIconRect;
        RectTransform panelRect = _panel.rectTransform;

        float yPivotValue = cursorPointer.position.y >= Screen.height / 2 ? 1 : 0;
        float pivotDifference = yPivotValue - panelRect.pivot.y;

        Vector2 anchoredPosition = panelRect.anchoredPosition;
        anchoredPosition.y += pivotDifference * panelRect.rect.height;

        panelRect.pivot = new(panelRect.pivot.x, yPivotValue);
        panelRect.anchoredPosition = anchoredPosition;
    }
    private void UpdatePosition_CursorPoint()
    {
        RectTransform cursorPointer = GameManager.instance.cursor.pointerIconRect;

        float offsetX = cursorPointer.position.x >= Screen.width / 2f ? _flipSeperationDistanceX.y : _flipSeperationDistanceX.x;
        _panel.rectTransform.anchoredPosition = cursorPointer.anchoredPosition + new Vector2(offsetX, 0f);
    }

    public void ToggleOn_CursorPoint(bool toggle)
    {
        if (_toggleDelayCoroutine != null)
        {
            StopCoroutine(_toggleDelayCoroutine);
            _toggleDelayCoroutine = null;
        }
        if (toggle == false || Toggle_Restricted())
        {
            _panel.gameObject.SetActive(false);
            return;
        }
        _toggleDelayCoroutine = StartCoroutine(CursorPoint_ToggleDelay());
    }
    private IEnumerator CursorPoint_ToggleDelay()
    {
        yield return new WaitForSeconds(_toggleDelayTime);

        Update_yPivotState();
        UpdatePosition_CursorPoint();

        _panel.gameObject.SetActive(true);

        _toggleDelayCoroutine = null;
    }
}