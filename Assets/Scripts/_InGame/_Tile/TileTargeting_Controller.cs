using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileTargeting
{
    Tile pivotTile { get; }

    TileTargeting_Data targetingData { get; }
    int targetingCount { get; }

    bool Targeting_Available(Tile targetingTile);
}

public class TileTargeting_Controller : MonoBehaviour
{
    private ITileTargeting _toggledSource;
    public ITileTargeting toggledSource => _toggledSource;

    private bool _targetingToggleLock;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed -= UnToggle_Targeting_onMissClick;
        input.OnRightClickPressed -= UnToggle_Targeting;
        input.OnLeftClickPressed -= Target_Tile;

        GameManager manager = GameManager.instance;

        manager.stageManager.endTurnEventBus.UnRegister(UnToggle_Targeting);
        manager.tileManager.tileHoverEventBus.UnRegister(Update_TargetingTileIndicators);
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed += UnToggle_Targeting_onMissClick;
        input.OnRightClickPressed += UnToggle_Targeting;
        input.OnLeftClickPressed += Target_Tile;

        GameManager manager = GameManager.instance;

        manager.stageManager.endTurnEventBus.Register(0,UnToggle_Targeting);
        manager.tileManager.tileHoverEventBus.Register(0, Update_TargetingTileIndicators);
    }


    // Targeting
    public bool Toggle_Targeting(ITileTargeting targetingSource)
    {
        if (targetingSource == null) return false;
        if (_targetingToggleLock) return false;

        GameManager manager = GameManager.instance;

        if (manager.handInventory.dragDropData != null) return false;
        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return false;

        if (targetingSource.targetingCount <= 0) return false;
        if (_toggledSource != null && _toggledSource != targetingSource) return false;

        bool toggled = targetingSource.targetingData.Toggle_Targeting(targetingSource.pivotTile);

        _toggledSource = toggled ? targetingSource : null;

        Update_TargetingTileIndicators();
        Update_InfoText();

        return toggled;
    }

    public void UnToggle_Targeting()
    {
        if (_toggledSource == null) return;

        _toggledSource.targetingData.Toggle_Targeting(null);
        _toggledSource = null;

        GameManager.instance.tileManager.Reset_TileIndicators();
        Update_InfoText();
    }
    private void UnToggle_Targeting(bool isPressed)
    {
        if (isPressed == false) return;

        UnToggle_Targeting();
    }
    private void UnToggle_Targeting_onMissClick(bool isPressed)
    {
        if (isPressed == false) return;
        if (GameManager.instance.tileManager.hoveringTile != null) return;

        UnToggle_Targeting();
    }

    private bool Targeting_Complete()
    {
        if (_toggledSource == null) return false;

        TileTargeting_Data targetingData = _toggledSource.targetingData;
        int targetSelectCount = _toggledSource.targetingCount;

        return targetingData.targetingTiles.Count >= targetSelectCount;
    }
    public IEnumerator TargetingToggle_LockUpdate()
    {
        _targetingToggleLock = true;
        yield return null;

        _targetingToggleLock = false;
    }

    private void Update_TargetingTileIndicators()
    {
        if (_toggledSource == null) return;

        TileManager tileManager = GameManager.instance.tileManager;
        tileManager.Reset_TileIndicators();

        if (Targeting_Complete()) return;

        Tile hoveringTile = tileManager.hoveringTile;
        List<Tile> targetingTiles = new(_toggledSource.targetingData.targetingTiles);

        for (int i = 0; i < targetingTiles.Count; i++)
        {
            targetingTiles[i].indicatorAnimController.Play_State(UIAnimation.Available);
        }

        if (hoveringTile == null || hoveringTile == _toggledSource.pivotTile || targetingTiles.Contains(hoveringTile)) return;

        string playStateString = _toggledSource.Targeting_Available(hoveringTile) ? UIAnimation.Toggle : UIAnimation.Restricted;
        hoveringTile.indicatorAnimController.Play_State(playStateString);
    }
    private void Target_Tile(bool isPressed)
    {
        if (isPressed == false || _toggledSource == null) return;

        Tile selectedTile = GameManager.instance.tileManager.hoveringTile;
        if (selectedTile == null) return;

        Tile pivotTile = _toggledSource.pivotTile;
        if (pivotTile == null || selectedTile == pivotTile) return;
       
        if (_toggledSource.Targeting_Available(selectedTile) == false) return;
        _toggledSource.targetingData.Target_Tile(selectedTile);

        if (Targeting_Complete() == false)
        {
            Update_TargetingTileIndicators();
            Update_InfoText();
            return;
        }
        UnToggle_Targeting();
        StartCoroutine(TargetingToggle_LockUpdate());
    }


    // Cursor UI
    private void Update_InfoText()
    {
        Cursor cursor = GameManager.instance.cursor;

        if (_toggledSource == null)
        {
            cursor.Update_InfoText(null);
            return;
        }

        int maxTargetingCount = _toggledSource.targetingCount;
        int targetingTileCount = _toggledSource.targetingData.targetingTiles.Count;

        string updateInfo = targetingTileCount < maxTargetingCount ? targetingTileCount + "/" + maxTargetingCount : null;
        cursor.Update_InfoText(updateInfo);
    }
}