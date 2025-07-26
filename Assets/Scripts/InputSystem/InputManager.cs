using UnityEngine;

public abstract class InputManager : MonoBehaviour
{
    protected InputSystem_Actions inputActions;

    public void Init(InputSystem_Actions inputSystemActions)
    {
        inputActions = inputSystemActions;
        SetCallBack();
    }
    protected abstract void SetCallBack();
    protected abstract void RemoveCallBack();
    public abstract void EnableInput();
    public abstract void DisableInput();
    private void OnDestroy()
    {
        // Remove os callbacks e destroi o Input System
        if (inputActions != null)
        {
            RemoveCallBack();
            inputActions.Dispose();
        }
    }

}