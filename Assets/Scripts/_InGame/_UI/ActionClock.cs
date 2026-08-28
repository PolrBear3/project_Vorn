using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionClock : MonoBehaviour
{
    [SerializeField] private Animator_Controller _animController;

    [Space(10)]
    [SerializeField][Range(0, 10)] private float _toggleDelayTime;

    private Coroutine _toggleDelayCoroutine;



    // Main
    public void Toggle(bool toggleAnimation)
    {
        bool isDelay = _toggleDelayTime > 0;
        
        _animController.sr.color = toggleAnimation & isDelay == false ? Color.white : Color.clear;
        
        if (_toggleDelayCoroutine != null)
        {
            StopCoroutine(_toggleDelayCoroutine);
            _toggleDelayCoroutine = null;
        }

        if (toggleAnimation == false)
        {
            _animController.StopCurrent_PlayingState();
            return;
        }

        if (isDelay)
        {
            _toggleDelayCoroutine = StartCoroutine(Toggle_DelayUpdate());
            return;
        }

        _animController.sr.color = toggleAnimation ? Color.white : Color.clear;
        _animController.Play_State(UIAnimation.Toggle);
    }

    private IEnumerator Toggle_DelayUpdate()
    {
        yield return new WaitForSeconds(_toggleDelayTime);

        _animController.sr.color = Color.white;
        _animController.Play_State(UIAnimation.Toggle);

        _toggleDelayCoroutine = null;
        yield break;
    }
}