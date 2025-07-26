using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Classe que gerencia os inputs do player usando o Input System gerado automaticamente
/// Implementa a interface IPlayerActions para receber callbacks dos inputs
/// </summary>
public class PlayerInputManager : InputManager, InputSystem_Actions.IPlayerActions
{
    [Header("Input Settings")]
    [SerializeField] private bool enableInputOnStart = true;
    [SerializeField] private CinemachineCamera cinemachineInputProvider;
    
    // Propriedades para acessar os valores atuais dos inputs
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsInteracting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsSprinting { get; private set; }
    
    // Eventos que outros scripts podem escutar
    public System.Action<Vector2> OnMoveEvent;
    public System.Action<Vector2> OnLookEvent;
    public System.Action OnAttackStarted;
    public System.Action OnAttackCanceled;
    public System.Action OnInteractStarted;
    public System.Action OnInteractCanceled;
    public System.Action OnCrouchStarted;
    public System.Action OnCrouchCanceled;
    public System.Action OnJumpStarted;
    public System.Action OnJumpCanceled;
    public System.Action OnSprintStarted;
    public System.Action OnSprintCanceled;
    public System.Action OnPreviousPressed;
    public System.Action OnNextPressed;
    public System.Action OnOpenInventory;
    
    private void Start()
    {
        if (enableInputOnStart)
        {
            EnableInput();
        }
    }

    protected override void SetCallBack()
    {
        inputActions.Player.SetCallbacks(this);
    }
    protected override void RemoveCallBack()
    {
        inputActions.Player.RemoveCallbacks(this);
    }

    public override void EnableInput()
    {
        inputActions.Player.Enable();
        cinemachineInputProvider.enabled = true;
    }

    public override void DisableInput()
    {
        inputActions.Player.Disable();
        cinemachineInputProvider.enabled = false;
        ResetInputValues();
    }

    #region Public Methods
    
    public void ResetInputValues()
    {
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        IsAttacking = false;
        IsInteracting = false;
        IsCrouching = false;
        IsJumping = false;
        IsSprinting = false;
    }
    
    #endregion
    
    #region IPlayerActions Implementation
    
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        OnMoveEvent?.Invoke(MoveInput);
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
        OnLookEvent?.Invoke(LookInput);
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsAttacking = true;
            OnAttackStarted?.Invoke();
        }
        else if (context.canceled)
        {
            IsAttacking = false;
            OnAttackCanceled?.Invoke();
        }
    }
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsInteracting = true;
            OnInteractStarted?.Invoke();
        }
        else if (context.canceled)
        {
            IsInteracting = false;
            OnInteractCanceled?.Invoke();
        }
    }
    
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsCrouching = true;
            OnCrouchStarted?.Invoke();
        }
        else if (context.canceled)
        {
            IsCrouching = false;
            OnCrouchCanceled?.Invoke();
        }
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsJumping = true;
            OnJumpStarted?.Invoke();
        }
        else if (context.canceled)
        {
            IsJumping = false;
            OnJumpCanceled?.Invoke();
        }
    }
    
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsSprinting = true;
            OnSprintStarted?.Invoke();
        }
        else if (context.canceled)
        {
            IsSprinting = false;
            OnSprintCanceled?.Invoke();
        }
    }
    
    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPreviousPressed?.Invoke();
        }
    }
    
    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnNextPressed?.Invoke();
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnOpenInventory?.Invoke();
        }
    }

    #endregion
}
