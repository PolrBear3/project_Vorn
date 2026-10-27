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

public class EnvironmentManager : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CameraSize_Data[] _cameraSizeDatas;

    [Space(20)]
    [SerializeField] private SpriteRenderer _materialBackground;
    [SerializeField][Range(0, 10)] private float _backgroundEffectSpeed;

    [Space(20)]
    [SerializeField] private Animator _transitionScreenAnimator;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);

        EventBus_GlobalController.Register(EventBus.AwakeLoad, Update_BackgroundSize);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Run_BackgroundEffects);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Toggle_TransitionScreen);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);

        EventBus_GlobalController.UnRegister(Update_BackgroundSize);
        EventBus_GlobalController.UnRegister(Run_BackgroundEffects);
        EventBus_GlobalController.UnRegister(Toggle_TransitionScreen);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        StageManager stageManager = GameManager.instance.stageManager;
        stageManager.setStageEventBus.Register(0, Update_CameraSize);

        stageManager.endTurnEventBus.Register(TransitionScreen_Playing);
        stageManager.endStageEventBus.Register(TransitionScreen_Playing);
    }

    public void UnSubscribe_All()
    {
        StageManager stageManager = GameManager.instance.stageManager;
        stageManager.setStageEventBus.UnRegister(Update_CameraSize);

        stageManager.endTurnEventBus.UnRegister(TransitionScreen_Playing);
        stageManager.endStageEventBus.UnRegister(TransitionScreen_Playing);
    }


    // Camera
    private void Update_CameraSize()
    {
        Stage_ScrObj currentStage = GameManager.instance.stageManager.stageMap.data.Current_StageData().stage;
        if (currentStage is not BattleStage_ScrObj battleStage) return;

        int rowTileCount = battleStage.rowTileCount;
        int columnTileCount = battleStage.columnTileCount;

        for (int i = 0; i < _cameraSizeDatas.Length; i++)
        {
            CameraSize_Data data = _cameraSizeDatas[i];
            if (rowTileCount > data.rowTileCount || columnTileCount > data.columnTileCount) continue;

            _mainCamera.orthographicSize = data.cameraSize;
            return;
        }
        _mainCamera.orthographicSize = _cameraSizeDatas[_cameraSizeDatas.Length - 1].cameraSize;
    }


    // Background
    private void Run_BackgroundEffects()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }
    private void Update_BackgroundSize()
    {
        float cameraHeight = _mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * _mainCamera.aspect;

        Vector2 spriteSize = _materialBackground.sprite.bounds.size;
        _materialBackground.transform.localScale = new Vector3(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y, 1f);
    }


    // Transition Screen
    public void Toggle_TransitionScreen(bool toggle)
    {
        _transitionScreenAnimator.Play(toggle ? UIAnimation.Toggle : UIAnimation.UnToggle);
    }
    private void Toggle_TransitionScreen()
    {
        Toggle_TransitionScreen(false);
    }

    public bool TransitionScreen_Playing()
    {
        AnimatorStateInfo stateInfo = _transitionScreenAnimator.GetCurrentAnimatorStateInfo(0);

        if (_transitionScreenAnimator.IsInTransition(0)) return true;
        return stateInfo.normalizedTime < 1f;
    }
}
