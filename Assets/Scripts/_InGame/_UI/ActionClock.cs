using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionClock : MonoBehaviour
{
    [SerializeField] private Animator_Controller _animController;


    // Main
    public void Toggle(bool toggleAnimation)
    {
        _animController.sr.color = toggleAnimation ? Color.white : Color.clear;
        
        if (toggleAnimation == false)
        {
            _animController.StopCurrent_PlayingState();
            return;
        }
        _animController.Play_State(UIAnimation.Toggle);
    }
}