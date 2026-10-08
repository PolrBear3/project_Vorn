using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel_ToggleController : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private RectTransform _canvasRect;

    [Space(20)]
    [SerializeField] private Direction _togglePosition;

    [SerializeField] private LeanTweenType _toggleTweenType;
    [SerializeField][Range(0, 10)] private float _toggleDuration;


    private RectTransform _togglePanelRect;

    private Vector2 _toggledPosition;
    private Vector2 _unToggledPosition;

    private bool _toggled;
    public Action<bool> OnToggle;


    // MonoBehaviour
    private void Awake()
    {
        if (gameObject.TryGetComponent(out RectTransform transform) == false) return;
        _togglePanelRect = transform;

        _toggledPosition = _togglePanelRect.anchoredPosition;
        _unToggledPosition = UnToggled_Position();

        _togglePanelRect.anchoredPosition = _unToggledPosition;
    }


    // Toggle
    private Vector2 UnToggled_Position()
    {
        Vector2 position = _toggledPosition;

        switch (_togglePosition)
        {
            case Direction.Left:
                position.x -= _canvasRect.rect.width + _togglePanelRect.rect.width;
                break;

            case Direction.Right:
                position.x += _canvasRect.rect.width + _togglePanelRect.rect.width;
                break;

            case Direction.Up:
                position.y += _canvasRect.rect.height + _togglePanelRect.rect.height;
                break;

            case Direction.Down:
                position.y -= _canvasRect.rect.height + _togglePanelRect.rect.height;
                break;
        }
        return position;
    }

    public void Toggle(bool toggle)
    {
        if (toggle == _toggled) return;

        _toggled = toggle;
        OnToggle?.Invoke(_toggled);

        LeanTween.cancel(_togglePanelRect);
        LeanTween.move(_togglePanelRect, toggle ? _toggledPosition : _unToggledPosition, _toggleDuration).setEase(_toggleTweenType);
    }
    public IEnumerator DelayToggle(bool toggle)
    {
        Toggle(toggle);
        while (LeanTween.isTweening(_togglePanelRect)) yield return null;
    }
}