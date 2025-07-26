using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputActionMapping
{
    public InputActionType actionType;
    public InputManager inputManager;
}

public enum InputActionType
{
    Player,
    GameplayMenu,
}

public class InputActionsController : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [SerializeField] private List<InputActionMapping> inputActionMappings = new List<InputActionMapping>();
    [SerializeField] private InputActionType defaultEnabledInput = InputActionType.Player;
    private Dictionary<InputActionType, InputManager> inputActionsMap;
    public static InputActionsController Instance { get; private set; }
    public Action OnInputActionsInitialized;
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        inputActions = new InputSystem_Actions();
        InitializeInputActionsMap();
    }

    void OnDestroy()
    {
        if (inputActionsMap != null)
        {
            foreach (var inputManager in inputActionsMap.Values)
            {
                if (inputManager != null)
                {
                    inputManager.DisableInput();
                }
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static InputManager GetActionMap(InputActionType actionType)
    {
        if (Instance != null && Instance.inputActionsMap.TryGetValue(actionType, out var inputManager))
        {
            return inputManager;
        }
        return null;
    }

    public static PlayerInputManager GetPlayerInputManager()
    {
        return GetActionMap(InputActionType.Player) as PlayerInputManager;
    }
    public static GameplayMenuInputManager GetGameplayMenuInputManager()
    {
        return GetActionMap(InputActionType.GameplayMenu) as GameplayMenuInputManager;
    }

    private void InitializeInputActionsMap()
    {
        inputActionsMap = new Dictionary<InputActionType, InputManager>();
        Debug.Log("Initializing Input Actions Map with " + inputActionMappings.Count + " mappings.");
        foreach (var mapping in inputActionMappings)
        {
            if (mapping.inputManager != null)
            {
                mapping.inputManager.Init(inputActions);
                inputActionsMap[mapping.actionType] = mapping.inputManager;
            }
        }
        if (inputActionsMap.TryGetValue(defaultEnabledInput, out InputManager defaultInputManager))
        {
            defaultInputManager.EnableInput();
        }
        Debug.Log("Input Actions Map initialized with " + inputActionsMap.Count + " action types.");
        OnInputActionsInitialized?.Invoke();
    }

    public InputManager GetInputManager(InputActionType actionType)
    {
        return inputActionsMap.TryGetValue(actionType, out InputManager inputManager) ? inputManager : null;
    }

    public void SetInputActionState(InputActionType actionType, bool enable)
    {
        if (inputActionsMap.TryGetValue(actionType, out InputManager inputManager))
        {
            if (enable)
            {
                inputManager.EnableInput();
            }
            else
            {
                inputManager.DisableInput();
            }
        }
    }
}