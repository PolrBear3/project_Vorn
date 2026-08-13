using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_Controller : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private Animator _animator;

    [Space(20)]
    [SerializeField] private string _stopStateName;
    [SerializeField] private string[] _stateNames;

    private string _currentState;


    // Main
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
        _animator.Play(_stopStateName, 0, 0f);
    }

    public void Play_State(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= _stateNames.Length) return;
        string stateToPlay = _stateNames[stateIndex];

        _currentState = stateToPlay;
        _animator.Play(stateToPlay, 0, 0f);
    }
}