using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour, ISubscriptionHandler
{
    [Space(10)]
    [SerializeField] private RectTransform _pointerIconRect;
    public RectTransform pointerIconRect => _pointerIconRect;

    [Space(20)]
    [SerializeField][Range(0, 1000)] private float _pointerIdleUpdateDistance;
    [SerializeField][Range(0, 10)] private float _pointerIdleDelayTime;

    [Space(20)]
    [SerializeField] private Image _pointerIconImage;

    [Space(20)]
    [SerializeField] private Image _infoTextBox;
    [SerializeField] private TextMeshProUGUI _infoText;

    [Space(20)]
    [SerializeField] private GameObject _handCardPrefab;

    [Space(0)]
    [SerializeField] private RectTransform _draggingCardFollowPoint;
    [SerializeField][Range(0, 100)] private float _draggingCardMovementSpeed;

    [Space(20)]
    [SerializeField] private ToolTip _dragCardToolTip;


    private Color _defaultInfoTextColor;

    private bool _pointerIconToggled;
    private Vector2 _pointerIdlePosition;

    private HandCard _draggingCard;
    public HandCard draggingCard => _draggingCard;

    private Coroutine _pointerIdleUpdateDelay;
    private bool _pointerIdleUpdated;


    public Action<bool> OnPointerIdle;


    // MonoBehaviour
    private void Awake()
    {
        _defaultInfoTextColor = _infoText.color;

        Hide_Cursor();
        Toggle_PointerIcon(true);
        Update_InfoText(null);

        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void Update()
    {
        DraggingCard_MovementUpdate();
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Hide_Cursor;
        input.OnLeftClickPressed += UpdatePointerIcon_OnClick;

        input.OnCursorControl += PointerIcon_MovementUpdate;
        input.OnCursorControl += PointerIcon_IdleUpdate;

        OnPointerIdle += Update_DraggingCard_ToolTip;
    }

    public void UnSubscribe_All()
    {
        // from Set_Data
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Hide_Cursor;
        input.OnLeftClickPressed -= UpdatePointerIcon_OnClick;

        input.OnCursorControl -= PointerIcon_MovementUpdate;
        input.OnCursorControl -= PointerIcon_IdleUpdate;

        OnPointerIdle -= Update_DraggingCard_ToolTip;
    }


    // Pointer
    private void Hide_Cursor()
    {
        UnityEngine.Cursor.visible = false;
    }

    private void PointerIcon_MovementUpdate(Vector2 cursorPosition)
    {
        if (_pointerIconToggled == false) return;
        _pointerIconRect.transform.position = cursorPosition;
    }

    private void PointerIcon_IdleUpdate(Vector2 cursorPosition)
    {
        if (Vector2.Distance(_pointerIdlePosition, cursorPosition) <= _pointerIdleUpdateDistance) return;
        _pointerIdlePosition = cursorPosition;

        if (_pointerIdleUpdateDelay != null) StopCoroutine(_pointerIdleUpdateDelay);

        bool wasIdle = _pointerIdleUpdated;

        _pointerIdleUpdated = false;
        _pointerIdleUpdateDelay = StartCoroutine(PointerIcon_IdleUpdateDelay());

        if (wasIdle == false) return;
        OnPointerIdle?.Invoke(false);
    }
    private IEnumerator PointerIcon_IdleUpdateDelay()
    {
        yield return new WaitForSecondsRealtime(_pointerIdleDelayTime);

        _pointerIdleUpdated = true;
        _pointerIdleUpdateDelay = null;

        OnPointerIdle?.Invoke(true);
    }

    private void Toggle_PointerIcon(bool toggle)
    {
        _pointerIconToggled = toggle;
        _pointerIconImage.gameObject.SetActive(_pointerIconToggled);
    }
    private void UpdatePointerIcon_OnClick(bool isPressing)
    {
        Vector2 updatePos = isPressing ? new Vector2(-6.25f, 6.25f) : Vector2.zero;

        _pointerIconImage.rectTransform.anchoredPosition = updatePos;
    }


    // Info Text
    public void Update_InfoText(string infoString)
    {
        bool toggle = infoString != null;
        _infoTextBox.gameObject.SetActive(toggle);

        if (toggle == false) return;

        _infoText.text = infoString;
        _infoText.color = _defaultInfoTextColor;
    }
    public void Update_InfoText(Color textColor)
    {
        _infoText.color = textColor;
    }


    // Card
    public bool Drag_Card(CardData dragCardData, Transform dragStartPosition)
    {
        if (_draggingCard != null) return false;

        GameObject dragCardObj = Instantiate(_handCardPrefab, dragStartPosition);
        dragCardObj.transform.SetParent(_draggingCardFollowPoint);

        if (dragCardObj.TryGetComponent(out HandCard dragCard) == false) return false;

        _draggingCard = dragCard;
        dragCard.Load(dragCardData);

        Update_DraggingCard_ManaCost();
        return true;
    }
    private void DraggingCard_MovementUpdate()
    {
        if (_draggingCard == null) return;

        RectTransform dragCardRect = _draggingCard.rectTransform;
        float movementSpeed = _draggingCardMovementSpeed * Time.deltaTime;

        dragCardRect.anchoredPosition = Vector2.Lerp(dragCardRect.anchoredPosition, _pointerIconRect.anchoredPosition, movementSpeed);
    }

    private void Update_DraggingCard_ManaCost()
    {
        Update_InfoText(_draggingCard != null ? "<sprite=5>" + _draggingCard.data.currentManaPrice.ToString() : null);
        Update_InfoText(GameManager.instance.heroManager.manaPanel.valueText.color);
    }
    private void Update_DraggingCard_ToolTip(bool toggle)
    {
        bool toolTipToggle = _draggingCard != null && toggle;
        _dragCardToolTip.ToggleOn_CursorPoint(toolTipToggle);

        if (toolTipToggle == false) return;

        CardData dragCardData = _draggingCard.data;
        Card_ScrObj cardScrObj = dragCardData.cardScrObj;

        string foramtDescription = Utility.FormatUpdate_Description(cardScrObj.cardDescription, dragCardData.currentData.DescriptionFormats());
        _dragCardToolTip.Update_Contents(null, cardScrObj.contentSprite, cardScrObj.cardName, foramtDescription);
    }

    public void Drop_Card()
    {
        if (_draggingCard == null) return;

        Destroy(_draggingCard.gameObject);
        _draggingCard = null;

        Update_DraggingCard_ManaCost();
        Update_DraggingCard_ToolTip(false);
    }
}
