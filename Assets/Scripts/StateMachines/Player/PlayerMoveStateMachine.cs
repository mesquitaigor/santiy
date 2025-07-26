using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerMoveStateMachine : StateMachine
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CinemachineCamera cinemachineFreeLook;
    [SerializeField] private InputActionsController inputActionsController;
    [NonSerialized] public PlayerInputManager playerInputManager;
    public Animator Animator => animator;
    public CharacterController CharacterController => characterController;
    public CinemachineCamera CinemachineFreeLook => cinemachineFreeLook;
    void Awake()
    {
        if (inputActionsController == null)
        {
            Debug.LogError("InputActionsController is null in PlayerMoveStateMachine.Awake()", this);
        }
        inputActionsController.OnInputActionsInitialized += InitializeController;
    }
    private void InitializeController()
    {
        playerInputManager = InputActionsController.GetPlayerInputManager();
        if (playerInputManager == null)
        {
            Debug.LogError("PlayerInputManager is null in PlayerMoveStateMachine.InitializeController()", this);
        }
    }
    
    void Start()
    {
        ChangeState(new PlayerMoveIdleState(this));
    }
}