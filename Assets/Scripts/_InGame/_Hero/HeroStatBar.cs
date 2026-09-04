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

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _widthPixelSize;
    [SerializeField][Range(0, 100)] private int _heightPixelSize;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _xSeperatePixelDistance;
    [SerializeField][Range(0, 100)] private int _ySeperatePixelDistance;

    [Space(10)]
    [SerializeField] private int _maxRowBlockCount;


    private List<Image> _currentBlocks = new();
    public List<Image> currentBlocks => _currentBlocks;


    // MonoBehaviour
    private void Awake()
    {
        _materialBackground.material.SetFloat("_Speed", _backgroundEffectSpeed);
    }

    private void OnApplicationQuit()
    {
        _materialBackground.material.SetFloat("_Speed", 0f);
    }


    // Blocks
    private int HeightPosition_BlockCount(float heightPosition)
    {
        int blockCount = 0;

        for (int i = 0; i < _currentBlocks.Count; i++)
        {
            if (_currentBlocks[i].rectTransform.anchoredPosition.y != heightPosition) continue; // RowIndex ?
            blockCount++;

            if (blockCount >= _maxRowBlockCount) break;
        }
        return blockCount;
    }
    private Vector2 Block_DropPosition()
    {
        float pixelValue = Utility.screenSpacePixelValue;
        float maxHeight = _heightPixelSize * pixelValue / 2;

        // get starting height position
        float dropHeightPos = -maxHeight;
        float heightUpdateValue = _ySeperatePixelDistance * pixelValue;

        // get available height position
        for (int i = 0; i < 999; i++) // get max hieght ?
        {
            if (HeightPosition_BlockCount(dropHeightPos) < _maxRowBlockCount) break;

            dropHeightPos += heightUpdateValue;
            if (dropHeightPos > maxHeight) break;
        }

        // get x position in width range
        float widthRange = _widthPixelSize * pixelValue / 2;

        return Vector2.zero;
    }

    private bool Block_Overlapping(Image checkBlock)
    {
        return false;
    }
    private void Resolve_BlockCollisions()
    {

    }

    public void Add_Block()
    {
        // GameObject spawnBlock = Instantiate();
    }
}