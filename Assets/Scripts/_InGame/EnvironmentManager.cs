using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraSize_Data
{
    [SerializeField][Range(0, 10)] private int _cameraSize;
    public int cameraSize => _cameraSize;

    [Space(10)]
    [SerializeField][Range(0, 50)] private int _rowTileCount;
    public int rowTileCount => _rowTileCount;

    [SerializeField][Range(0, 50)] private int _columnTileCount;
    public int columnTileCount => _columnTileCount;
}

public class EnvironmentManager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Camera _camera;
    [SerializeField] private CameraSize_Data[] _cameraSizeDatas;

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
        EventBus_Controller stageSet = GameManager.instance.stageManager.stageSetEventBus;

        stageSet.UnRegister(Update_CameraSize);

        stageSet.UnRegister(Update_BackgroundSize);
        stageSet.UnRegister(Run_BackgroundEffects);
    }


    // Data
    private void Set_Data()
    {
        EventBus_Controller stageSet = GameManager.instance.stageManager.stageSetEventBus;

        stageSet.Register(0, Update_CameraSize);

        stageSet.Register(0, Update_BackgroundSize);
        stageSet.Register(0, Run_BackgroundEffects);
    }


    // Main
    private void Run_BackgroundEffects()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }
    private void Update_BackgroundSize()
    {
        float cameraHeight = _camera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * _camera.aspect;

        Vector2 spriteSize = _materialBackground.sprite.bounds.size;
        _materialBackground.transform.localScale = new Vector3(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y, 1f);
    }

    private void Update_CameraSize()
    {
        Stage_ScrObj currentStage = GameManager.instance.currentGameData.stage;
        if (currentStage is not BattleStage_ScrObj battleStage) return;

        int rowTileCount = battleStage.rowTileCount;
        int columnTileCount = battleStage.columnTileCount;

        for (int i = 0; i < _cameraSizeDatas.Length; i++)
        {
            CameraSize_Data data = _cameraSizeDatas[i];
            if (rowTileCount > data.rowTileCount || columnTileCount > data.columnTileCount) continue;

            _camera.orthographicSize = data.cameraSize;
            return;
        }
        _camera.orthographicSize = _cameraSizeDatas[_cameraSizeDatas.Length - 1].cameraSize;
    }
}
