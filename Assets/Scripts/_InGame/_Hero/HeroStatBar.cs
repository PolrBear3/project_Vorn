using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStatBar : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _materialBackground;
    [SerializeField][Range(0, 10)] private float _backgroundEffectSpeed;

    [Space(20)]
    [SerializeField] private GameObject _statBlockPrefab;
    [SerializeField] private Sprite _statBlockSprite;


    // MonoBehaviour
    private void Awake()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }

    private void OnApplicationQuit()
    {
        _materialBackground.material.SetFloat("_Speed", 0f);
    }
}
