using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMovement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private LeanTweenType _movementTweenType;
    [SerializeField][Range(0, 10)] private float _singleTileMovementTime;

    [Space(20)]
    [SerializeField] private SpriteRenderer[] _directionFlipRenderers;


    private Tile _currentTile;
    public Tile currentTile => _currentTile;

    private Coroutine _movementCoroutine;
    public Coroutine movementCoroutine => _movementCoroutine;


    // Data
    public bool Set_CurrentTile(Tile setTile)
    {
        if (setTile == null) return false;

        _currentTile = setTile;
        return true;
    }

    
    // Flip
    public void Direction_FlipUpdate(float faceDirection)
    {
        bool isFlip = faceDirection < 0;
        
        for (int i = 0; i < _directionFlipRenderers.Length; i++)
        {
            _directionFlipRenderers[i].flipX = isFlip;
        }
    }
    public void Direction_FlipUpdate(Tile faceTile)
    {
        if (_currentTile == null) return;
        
        Direction_FlipUpdate(faceTile.data.position.x - _currentTile.data.position.x);
    }


    // Movement
    public void Moveto_Tile(Tile destinationTile, Vector2 offset)
    {
        if (destinationTile != null && destinationTile.currentOccupant != null) return;

        Tile previousTile = _currentTile;

        if (Set_CurrentTile(destinationTile) == false) return;
        _currentTile.Set_Occupant(gameObject);

        if (_movementCoroutine != null)
        {
            LeanTween.cancel(gameObject);

            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        if (previousTile == null)
        {
            transform.position = (Vector2)_currentTile.transform.position + offset;
            return;
        }
        previousTile.Set_Occupant(null);

        _movementCoroutine = StartCoroutine(Movement_Update(previousTile, _currentTile, offset));
    }
    public void Moveto_Tile(Tile destination)
    {
        Moveto_Tile(destination, Vector2.zero);
    }

    private IEnumerator Movement_Update(Tile previousTile, Tile destinationTile, Vector2 offset)
    {
        int distance = Utility.Chebyshev_Distance(previousTile.transform.position, destinationTile.transform.position);
        float movementTime = _singleTileMovementTime * distance;

        LeanTween.move(gameObject, (Vector2)destinationTile.transform.position + offset, movementTime).setEase(_movementTweenType);
        while (LeanTween.isTweening(gameObject)) yield return null;

        _movementCoroutine = null;
        yield break;
    }
}