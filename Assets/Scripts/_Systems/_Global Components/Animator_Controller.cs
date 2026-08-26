using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_Controller : MonoBehaviour
{
    private SpriteRenderer _sr;
    public SpriteRenderer sr => _sr;

    private Animator _animator;

    private Sprite _defaultSprite;
    private const string None = "None";

    private string _currentState;


    // MonoBehaviour
    private void Awake()
    {
        if (gameObject.TryGetComponent(out Animator anim) == false) return;
        _animator = anim;

        if (gameObject.TryGetComponent(out SpriteRenderer sr) == false) return;
        _sr = sr;
        
        _defaultSprite = _sr.sprite;
    }


    // Main
    public bool CurrentState_Playing(string stateName)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }
    public bool CurrentState_Playing()
    {
        if (_currentState == null) return false;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(_currentState) == false) return false;

        if (stateInfo.loop) return true;
        return stateInfo.normalizedTime < 1f;
    }

    public void StopCurrent_PlayingState()
    {
        _currentState = null;
        _animator.Play(None, 0, 0f);
        _sr.sprite = _defaultSprite;
    }

    public void Play_State(string stateName)
    {
        _currentState = stateName;
        _animator.Play(stateName, 0, 0f);
    }
}