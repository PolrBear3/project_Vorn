using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Plugins.Animate_UI_Materials;
using System;

public abstract class HoverUI : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private Image _image;
    public Image image => _image;

    [Space(20)]
    [SerializeField] private GraphicPropertyOverrideColor _greyscaleTint;
    public GraphicPropertyOverrideColor greyscaleTint => _greyscaleTint;

    [SerializeField] private GraphicPropertyOverrideRange _greyscaleBlend;
    public GraphicPropertyOverrideRange greyscaleBlend => _greyscaleBlend;

    [Space(20)]
    [SerializeField] private Animator_Controller _animController;

    [SerializeField] private EventSystems_Controller _hoverController;
    public EventSystems_Controller hoverController => _hoverController;


    private Color _lockColor;
    private bool _hoverLocked;


    // MonoBehaviour
    public virtual void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        if (_greyscaleTint == null) return;
        _lockColor = _greyscaleTint.PropertyValue;
    }

    public virtual void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public virtual void Subscribe_All()
    {
        if (_hoverController == null) return;
        _hoverController.OnPointerState += Update_OnHover;
    }

    public virtual void UnSubscribe_All()
    {
        if (_hoverController == null) return;
        _hoverController.OnPointerState -= Update_OnHover;
    }


    // Interaction
    public void Update_UnlockVisuals(bool unlock)
    {
        if (_greyscaleTint == null || _greyscaleBlend == null) return;
        _greyscaleBlend.PropertyValue = unlock ? 0 : 1f;

        if (unlock) return;
        _greyscaleTint.PropertyValue = _lockColor;
    }


    public void Toggle_HoverLock(bool toggle)
    {
        _hoverLocked = toggle;

        if (_hoverLocked == false) return;

        if (_animController == null) return;
        if (_animController.CurrentState_Playing() == false || _animController.CurrentState_Playing(UIAnimation.UnToggle)) return;

        _animController.Play_State(UIAnimation.UnToggle);
    }

    public abstract void OnHover_Update(bool isHovering);
    private void Update_OnHover(bool isHovering)
    {
        if (_hoverLocked) return;

        OnHover_Update(isHovering);

        if (_animController == null) return;
        _animController.Play_State(isHovering ? UIAnimation.Toggle : UIAnimation.UnToggle);
    }


    public abstract void OnSelect_Update();
    public void Update_OnSelect()
    {
        OnSelect_Update();

        if (_animController == null) return;
        _animController.Play_State(UIAnimation.UnToggle);
    }
}
