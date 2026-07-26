using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystems_Controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnEnter;
    public Action OnExit;

    public Action<bool> OnPointerState;
    public Action<bool> OnPointerHoldState;

    private bool _pointerDetected;
    public bool pointerDetected => _pointerDetected;

    private const float _pointerHoldDelayTime = 0.5f; // match value of action interaction hold time

    private Coroutine _pointerHoldCoroutine;
    public Coroutine pointerHoldCoroutine => _pointerHoldCoroutine;


    // MonoBehaviour
    private void OnDisable()
    {
        OnPointerExit();
    }


    // EventSystems
    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerDetected = true;

        OnEnter?.Invoke();
        OnPointerState?.Invoke(_pointerDetected);

        _pointerHoldCoroutine = StartCoroutine(PointerHold_Delay());
    }

    private void OnPointerExit()
    {
        if (_pointerDetected == false) return;
        _pointerDetected = false;

        OnExit?.Invoke();
        OnPointerState?.Invoke(_pointerDetected);

        if (_pointerHoldCoroutine != null)
        {
            StopCoroutine(_pointerHoldCoroutine);
            _pointerHoldCoroutine = null;
        }
        OnPointerHoldState?.Invoke(false);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit();
    }

    private IEnumerator PointerHold_Delay()
    {
        yield return new WaitForSeconds(_pointerHoldDelayTime);

        OnPointerHoldState?.Invoke(true);
        _pointerHoldCoroutine = null;
    }
}
