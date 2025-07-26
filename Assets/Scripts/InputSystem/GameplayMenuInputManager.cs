using UnityEngine;
using UnityEngine.InputSystem;
public class GameplayMenuInputManager : InputManager, InputSystem_Actions.IGameplayMenuActions
{
    public System.Action OnClosePressed;
    public System.Action OnInventoryKeyPressed;

    private void Awake()
    {
        // A instância de inputActions será definida via Init() chamado pelo InputActionsController
    }
    
    public void OnCloseMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnClosePressed?.Invoke();
        }
    }

    public void OnInventoryKey(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnInventoryKeyPressed?.Invoke();
        }
    }

    protected override void SetCallBack()
    {
        if (inputActions != null)
        {
            inputActions.GameplayMenu.AddCallbacks(this);
        }
        else
        {
            Debug.LogError("GameplayMenuInputManager: inputActions is null in SetCallBack()");
        }
    }

    protected override void RemoveCallBack()
    {
        inputActions.GameplayMenu.RemoveCallbacks(this);
    }

    public override void EnableInput()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions is null in GameplayMenuInputManager.EnableInput()");
            return;
        }
        
        inputActions.GameplayMenu.Enable();
    }

    public override void DisableInput()
    {
        if (inputActions != null)
        {
            inputActions.GameplayMenu.Disable();
        }
    }
}