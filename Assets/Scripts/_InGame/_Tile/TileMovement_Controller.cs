using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMovement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private LeanTweenType _movementTweenType;
    [SerializeField][Range(0, 10)] private float _singleTileMovementTime;
    
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


    // Movement
    public void Moveto_Tile(Tile destinationTile, Vector2 offset)
    {
        Tile previousTile = _currentTile;

        if (Set_CurrentTile(destinationTile) == false) return;

        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        if (previousTile == null)
        {
            transform.position = (Vector2)_currentTile.transform.position + offset;
            return;
        }
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