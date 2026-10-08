using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuild_GroupCard : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _hoverDetector;

    [Space(20)]
    [SerializeField] private Image _baseImage;
    [SerializeField] private Image _contentImage;


    // UI
    public void Load_Images(Card_ScrObj cardToLoad)
    {
        bool toggle = cardToLoad != null;
        gameObject.SetActive(toggle);

        if (toggle == false) return;
        _contentImage.sprite = cardToLoad.contentSprite;
    }


    // Hover
}
