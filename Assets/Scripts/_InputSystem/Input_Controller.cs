using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

public class Input_Controller : MonoBehaviour
{
    public static Input_Controller instance;


    [Space(20)]
    [SerializeField] private PlayerInput _playerInput;
    public PlayerInput playerInput => _playerInput;

    [SerializeField] private ControlScheme_ScrObj[] _schemes;


    // Current Datas
    private string _currentSchemeName;

    private ControlScheme_ScrObj _currentScheme;
    public ControlScheme_ScrObj currentScheme => _currentScheme;

    private List<string> _actionMaps = new();


    // Player Input Update Observers
    public Action OnSchemeUpdate;
    public Action OnActionMapUpdate;


    // Keyboard Input Observers
    public Action<InputActionReference> OnAnyInput;

    public Action<Vector2> OnMovement;

    public Action<bool> OnInteractPressed;
    public Action OnInteract;
    public Action OnHoldInteract;

    public Action OnAction1;
    public Action OnAction2;

    public Action OnCancel;


    // Mouse Input Observers
    public Action<Vector2> OnCursorControl;

    public Action<bool> OnLeftClickPressed;
    public Action OnLeftClick;
    public Action OnHoldLeftClick;

    public Action OnRightClick;
    public Action OnHoldRightClick;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;

        Load_ActionMaps();
        Handle_SchemeUpdate(_playerInput);

        _playerInput.onControlsChanged += Handle_SchemeUpdate;
    }

    private void Update()
    {
        CurrentScheme_Update();
    }

    private void OnDestroy()
    {
        _playerInput.onControlsChanged -= Handle_SchemeUpdate;
    }


    // Data Control
    private void Load_ActionMaps()
    {
        for (int i = 0; i < _playerInput.actions.actionMaps.Count; i++)
        {
            _actionMaps.Add(_playerInput.actions.actionMaps[i].name);
        }
    }

    public void UpdateCurrent_ActionMap(int indexNum)
    {
        if (indexNum < 0 || indexNum >= _actionMaps.Count) return;

        indexNum = Mathf.Clamp(indexNum, 0, _actionMaps.Count - 1);
        string mapName = _actionMaps[indexNum];

        _playerInput.SwitchCurrentActionMap(mapName);

        OnActionMapUpdate?.Invoke();
    }
    public int Current_ActionMapNum()
    {
        string currentMapName = _playerInput.currentActionMap.name;

        for (int i = 0; i < _actionMaps.Count; i++)
        {
            if (currentMapName != _actionMaps[i]) continue;
            return i;
        }
        return 0;
    }

    public InputActionReference ActionReference(string actionName)
    {
        ActionKey_Data[] datas = _currentScheme.actionKeyDatas;

        for (int i = 0; i < datas.Length; i++)
        {
            if (datas[i].actionRef.action.name != actionName) continue;
            return datas[i].actionRef;
        }
        return null;
    }


    // Scheme Control
    private void CurrentScheme_Update()
    {
        if (_currentSchemeName == _playerInput.currentControlScheme) return;
        _currentSchemeName = _playerInput.currentControlScheme;

        Update_CurrentScheme(_currentSchemeName);
    }
    private void Handle_SchemeUpdate(PlayerInput playerInput)
    {
        Update_CurrentScheme(_playerInput.currentControlScheme);
    }

    public void Update_CurrentScheme(string schemeName)
    {
        _currentScheme = ControlScheme(schemeName);

        if (_currentScheme == null)
        {
            Debug.Log("Update Scheme Not Found!");
            return;
        }

        OnSchemeUpdate?.Invoke();
        Debug.Log("_currentScheme: " + _currentScheme.name + "/ _playerInput.currentControlScheme: " + _playerInput.currentControlScheme);
    }
    public void Update_EmojiAsset(TextMeshProUGUI text)
    {
        text.spriteAsset = _currentScheme.emojiAsset;
    }

    private ControlScheme_ScrObj ControlScheme(string name)
    {
        for (int i = 0; i < _schemes.Length; i++)
        {
            if (_schemes[i].schemeName != name) continue;
            return _schemes[i];
        }
        return null;
    }
    public GameObject CurrentScheme_ActionKey(InputActionReference reference)
    {
        ActionKey_Data[] datas = _currentScheme.actionKeyDatas;

        for (int i = 0; i < datas.Length; i++)
        {
            if (reference != datas[i].actionRef) continue;
            return datas[i].actionKey;
        }
        return null;
    }


    // KeyBoard Actions
    public void AnyInput(InputAction.CallbackContext context)
    {
        InputActionReference actionRef = ActionReference(context.action.name);
        if (actionRef == null) return;

        OnAnyInput?.Invoke(actionRef);
    }

    public void Movement(InputAction.CallbackContext context)
    {
        Vector2 directionInput = context.ReadValue<Vector2>();
        OnMovement?.Invoke(directionInput);
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnInteractPressed?.Invoke(true);
            return;
        }

        if (context.canceled == false) return;
        OnInteractPressed?.Invoke(false);
    }
    public void HoldInteract(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction == false)
        {
            OnInteract?.Invoke();
            return;
        }
        OnHoldInteract?.Invoke();
    }

    public void Action1(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnAction1?.Invoke();
    }
    public void Action2(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnAction2?.Invoke();
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnCancel?.Invoke();
    }


    // Mouse Actions
    public void CursorControl(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 positionInput = context.ReadValue<Vector2>();
        OnCursorControl?.Invoke(positionInput);
    }

    public void LeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnLeftClickPressed?.Invoke(true);
            return;
        }
        if (context.canceled == false) return;

        OnLeftClickPressed?.Invoke(false);
    }
    public void HoldLeftClick(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction == false)
        {
            OnLeftClick?.Invoke();
            return;
        }
        OnHoldLeftClick?.Invoke();
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            OnHoldRightClick?.Invoke();
            return;
        }
        OnRightClick?.Invoke();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Input_Controller))]
public class Input_Controller_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        Input_Controller controller = (Input_Controller)target;

        base.OnInspectorGUI();
        serializedObject.Update();

        GUILayout.Space(60);

        if (GUILayout.Button("Toggle Scheme"))
        {
            if (controller.currentScheme.schemeName == "PC")
            {
                controller.Update_CurrentScheme("GamePad");
                return;
            }
            controller.Update_CurrentScheme("PC");
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif