using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ToolTip : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private RectTransform _positionUpdateBoundary;
    
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
    private float _defaultNameFontSize;

    private Coroutine _toggleDelayCoroutine;

    private List<Func<bool>> _toggleRestrictionChecks = new();
    public List<Func<bool>> toggleRestrictionChecks => _toggleRestrictionChecks;


    // MonoBehaviour
    private void Awake()
    {
        _defaultBaseSprite = _baseImage.sprite;
        _defaultNameFontSize = _nameText.fontSize;

        UnToggle();
    }


    // Main
    private void Update_NameText(string nameString)
    {
        _nameText.text = nameString;
        _nameText.fontSize = _defaultNameFontSize;

        float availableWidth = _nameText.rectTransform.rect.width;

        float textBoxHeight = _nameText.rectTransform.rect.height;
        float preferredWidth = _nameText.GetPreferredValues(nameString, Mathf.Infinity, textBoxHeight).x;

        if (preferredWidth <= availableWidth) return;
        float sizeRatio = availableWidth / preferredWidth;

        _nameText.fontSize = Mathf.Max(0, _defaultNameFontSize * sizeRatio);
    }
    public void Update_Contents(Sprite baseSprite, Sprite iconSprite, string nameString, string descriptionString)
    {
        _baseImage.sprite = baseSprite != null ? baseSprite : _defaultBaseSprite;
        _iconImage.sprite = iconSprite;

        Update_NameText(nameString);
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

    public void UnToggle()
    {
        Toggle(false);
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

    public Vector2 PositionUpdateBoundary_OverflowAmount()
    {
        if (_panel == null || _positionUpdateBoundary == null)
            return Vector2.zero;

        RectTransform panelRect = _panel.rectTransform;

        Vector3[] panelCorners = new Vector3[4];
        Vector3[] boundaryCorners = new Vector3[4];

        panelRect.GetWorldCorners(panelCorners);
        _positionUpdateBoundary.GetWorldCorners(boundaryCorners);

        Vector2 worldOverflow = Vector2.zero;

        // Left
        if (panelCorners[0].x < boundaryCorners[0].x)
            worldOverflow.x = panelCorners[0].x - boundaryCorners[0].x;

        // Right
        if (panelCorners[2].x > boundaryCorners[2].x)
            worldOverflow.x = panelCorners[2].x - boundaryCorners[2].x;

        // Bottom
        if (panelCorners[0].y < boundaryCorners[0].y)
            worldOverflow.y = panelCorners[0].y - boundaryCorners[0].y;

        // Top
        if (panelCorners[2].y > boundaryCorners[2].y)
            worldOverflow.y = panelCorners[2].y - boundaryCorners[2].y;

        RectTransform panelParent = panelRect.parent as RectTransform;
        if (panelParent == null)
            return worldOverflow;

        Vector3 localOverflow =
            panelParent.InverseTransformVector(worldOverflow);

        return new Vector2(localOverflow.x, localOverflow.y);
    }
    private void UpdatePosition_CursorPoint()
    {
        GameManager manager = GameManager.instance;
        
        RectTransform cursorPointer = manager.cursor.pointerIconRect;
        RectTransform panelRect = _panel.rectTransform;

        float offsetX = cursorPointer.position.x >= Screen.width / 2f ? _flipSeperationDistanceX.y : _flipSeperationDistanceX.x;
        panelRect.anchoredPosition = cursorPointer.anchoredPosition + new Vector2(offsetX, 0f);

        panelRect.anchoredPosition -= PositionUpdateBoundary_OverflowAmount();
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