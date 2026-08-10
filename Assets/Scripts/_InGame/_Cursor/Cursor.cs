using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private RectTransform _pointerIconRect;
    public RectTransform pointerIconRect => _pointerIconRect;

    [Space(20)]
    [SerializeField] private Image _pointerIconImage;
    [SerializeField] private TextMeshProUGUI _infoText;

    [Space(20)]
    [SerializeField] private GameObject _handCardPrefab;

    [Space(20)]
    [SerializeField] private RectTransform _draggingCardFollowPoint;
    [SerializeField][Range(0, 100)] private float _draggingCardMovementSpeed;


    private bool _pointerIconToggled;

    private HandCard _draggingCard;
    public HandCard draggingCard => _draggingCard;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);

        Hide_Cursor();
        Toggle_PointerIcon(true);
        Update_InfoText(null);
    }

    private void Update()
    {
        DraggingCard_MovementUpdate();
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Hide_Cursor;
        input.OnCursorControl -= PointerIcon_MovementUpdate;
        input.OnLeftClickPressed -= UpdatePointerIcon_OnClick;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Hide_Cursor;
        input.OnCursorControl += PointerIcon_MovementUpdate;
        input.OnLeftClickPressed += UpdatePointerIcon_OnClick;
    }


    // Pointer
    private void Hide_Cursor()
    {
        UnityEngine.Cursor.visible = false;
    }

    private void Toggle_PointerIcon(bool toggle)
    {
        _pointerIconToggled = toggle;
        _pointerIconImage.gameObject.SetActive(_pointerIconToggled);
    }
    private void PointerIcon_MovementUpdate(Vector2 cursorPosition)
    {
        if (_pointerIconToggled == false) return;
        _pointerIconRect.transform.position = cursorPosition;
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
        _infoText.gameObject.SetActive(toggle);

        if (toggle == false) return;
        _infoText.text = infoString;
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

        return true;
    }
    private void DraggingCard_MovementUpdate()
    {
        if (_draggingCard == null) return;

        RectTransform dragCardRect = _draggingCard.rectTransform;
        float movementSpeed = _draggingCardMovementSpeed * Time.deltaTime;

        dragCardRect.anchoredPosition = Vector2.Lerp(dragCardRect.anchoredPosition, _pointerIconRect.anchoredPosition, movementSpeed);
    }

    public void Drop_Card()
    {
        if (_draggingCard == null) return;

        Destroy(_draggingCard.gameObject);
        _draggingCard = null;
    }
}