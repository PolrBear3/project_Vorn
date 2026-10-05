using System.Collections;
using UnityEngine;

public class Component_ToggleController : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Direction _togglePosition;
    [SerializeField] private LeanTweenType _toggleTweenType;
    [SerializeField][Range(0, 10)] private float _toggleDuration;

    [Space(10)]
    [SerializeField] private bool _stayToggledOnLoad;


    private Transform _componentTransform;

    private Vector3 _toggledPosition;
    private Vector3 _unToggledPosition;

    private bool _toggled;
    public bool toggled => _toggled;


    // MonoBehaviour
    private void Awake()
    {
        if (gameObject.TryGetComponent(out Transform transform) == false) return;
        _componentTransform = transform;

        _toggledPosition = _componentTransform.position;
        _unToggledPosition = UnToggled_Position();

        if (_stayToggledOnLoad) return;
        _componentTransform.position = _unToggledPosition;
    }


    // Toggle
    private Vector3 UnToggled_Position()
    {
        Camera mainCamera = Camera.main;

        float distance = Mathf.Abs(_componentTransform.position.z - mainCamera.transform.position.z);

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distance));

        Vector3 position = _toggledPosition;

        switch (_togglePosition)
        {
            case Direction.Left:
                position.x -= topRight.x - bottomLeft.x;
                break;

            case Direction.Right:
                position.x += topRight.x - bottomLeft.x;
                break;

            case Direction.Up:
                position.y += topRight.y - bottomLeft.y;
                break;

            case Direction.Down:
                position.y -= topRight.y - bottomLeft.y;
                break;
        }
        return position;
    }

    public void Toggle(bool toggle)
    {
        _toggled = toggle;
        LeanTween.move(_componentTransform.gameObject, toggle ? _toggledPosition : _unToggledPosition, _toggleDuration).setEase(_toggleTweenType);
    }
    public IEnumerator DelayToggle(bool toggle)
    {
        Toggle(toggle);
        while (LeanTween.isTweening(_componentTransform.gameObject)) yield return null;
    }
}