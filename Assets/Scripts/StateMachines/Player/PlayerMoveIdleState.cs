using UnityEngine;

public class PlayerMoveIdleState: PlayerMoveBaseState
{
    public PlayerMoveIdleState(PlayerMoveStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.Animator.SetBool("isMoving", false);
        
        // Usa a propriedade playerInputManager do stateMachine
        if (stateMachine.playerInputManager != null)
        {
            // Escuta o evento de movimento
            stateMachine.playerInputManager.OnMoveEvent += HandleMovementInput;
        }
        else
        {
            Debug.LogError("PlayerInputManager não encontrado no PlayerMoveStateMachine!");
        }
    }

    public override void Update(float deltaTime)
    {
        // Lógica adicional se necessário
    }
    
    private void HandleMovementInput(Vector2 moveInput)
    {
        // Se há input de movimento (magnitude maior que um threshold pequeno)
        if (moveInput.magnitude > 0.1f)
        {
            stateMachine.ChangeState(new PlayerMoveRunningState(stateMachine));
        }
    }

    public override void Exit()
    {
        // Remove a escuta do evento para evitar memory leaks
        if (stateMachine.playerInputManager != null)
        {
            stateMachine.playerInputManager.OnMoveEvent -= HandleMovementInput;
        }
    }
}