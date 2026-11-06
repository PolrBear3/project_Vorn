using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuild_ProgressionIcon : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Animator_Controller _animController;
    public Animator_Controller animController => _animController;

    [SerializeField] private EventSystems_Controller _hoverController;
    public EventSystems_Controller hoverController => _hoverController;


    // MonoBehaviour
    private void Awake()
    {
        _hoverController.OnPointerState += Update_OnHover;
    }

    private void OnDestroy()
    {
        _hoverController.OnPointerState -= Update_OnHover;
    }


    // Interaction
    private void Update_OnHover(bool isHovering)
    {
        GameManager.instance.deckBuildManager.Hover_ProgressionIcon(isHovering ? this : null);
    }
}
