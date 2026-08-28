using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _materialBackground;
    [SerializeField][Range(0, 10)]  private float _backgroundEffectSpeed;

    [Space(20)]
    [SerializeField] private SpriteRenderer _tilePlatform;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        GameManager manager = GameManager.instance;
        manager.tileManager.generateEventBus.UnRegister(Update_TilePlatform);
    }


    // Data
    private void Set_Data()
    {
        GameManager manager = GameManager.instance;
        manager.tileManager.generateEventBus.Register(0, Update_TilePlatform);


        Run_BackgroundEffects();
    }


    // Main
    private void Run_BackgroundEffects()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }

    private void Update_TilePlatform()
    {
        
    }
}
