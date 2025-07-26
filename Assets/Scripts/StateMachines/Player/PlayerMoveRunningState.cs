using UnityEngine;

public class PlayerMoveRunningState: PlayerMoveBaseState
{
    [Header("Movement Settings")]
    private float moveSpeed = 5f;
    private float gravity = -9.81f;
    private float terminalVelocity = -20f;
    
    [Header("Inertia Settings")]
    private float acceleration = 15f;
    private float deceleration = 10f;
    
    private Vector3 velocity;
    private Vector3 currentHorizontalVelocity;
    private Vector2 currentMoveInput;

    public PlayerMoveRunningState(PlayerMoveStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.Animator.SetBool("isMoving", true);
        
        if (stateMachine.playerInputManager != null)
        {
            stateMachine.playerInputManager.OnMoveEvent += HandleMovementInput;
        }
        else
        {
            Debug.LogError("PlayerInputManager não encontrado no PlayerMoveStateMachine!");
        }
    }

    public override void Update(float deltaTime)
    {
        ApplyMovement(deltaTime);
        
        ApplyGravity(deltaTime);
        
        stateMachine.CharacterController.Move(velocity * deltaTime);
        
        RotatePlayerBasedOnForwardMovement();
    }
    
    private void ApplyMovement(float deltaTime)
    {
        Vector3 targetVelocity = Vector3.zero;
        
        if (currentMoveInput.magnitude > 0.1f)
        {
            Transform cameraTransform = stateMachine.CinemachineFreeLook.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            Vector3 moveDirection = (cameraForward * currentMoveInput.y) + (cameraRight * currentMoveInput.x);
            moveDirection.Normalize();
            
            targetVelocity = moveDirection * moveSpeed;
        }
        
        float lerpSpeed = targetVelocity.magnitude > currentHorizontalVelocity.magnitude ? 
                         acceleration : deceleration;
        
        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetVelocity,
            lerpSpeed * deltaTime
        );
        
        velocity.x = currentHorizontalVelocity.x;
        velocity.z = currentHorizontalVelocity.z;
    }
    
    private void ApplyGravity(float deltaTime)
    {
        if (stateMachine.CharacterController.isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            velocity.y += gravity * deltaTime;
            
            if (velocity.y < terminalVelocity)
            {
                velocity.y = terminalVelocity;
            }
        }
    }
    
    private void RotatePlayerBasedOnForwardMovement()
    {
        if (currentMoveInput.y > 0.1f)
        {
            Transform cameraTransform = stateMachine.CinemachineFreeLook.transform;
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                stateMachine.transform.rotation = Quaternion.Slerp(
                    stateMachine.transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
        }
    }
    
    private void HandleMovementInput(Vector2 moveInput)
    {
        currentMoveInput = moveInput;
        
        Vector2 animationInput = CalculateAnimationInput(moveInput);
        
        stateMachine.Animator.SetFloat("moveX", animationInput.x);
        stateMachine.Animator.SetFloat("moveY", animationInput.y);
        
        if (moveInput.magnitude <= 0.1f)
        {
            stateMachine.ChangeState(new PlayerMoveIdleState(stateMachine));
        }
    }
    
    private Vector2 CalculateAnimationInput(Vector2 rawInput)
    {
        if (rawInput.magnitude <= 0.1f)
            return Vector2.zero;
        return rawInput;
    }

    public override void Exit()
    {
        stateMachine.Animator.SetBool("isMoving", false);
        
        stateMachine.Animator.SetFloat("moveX", 0f);
        stateMachine.Animator.SetFloat("moveY", 0f);
        
        velocity.x = 0f;
        velocity.z = 0f;
        currentHorizontalVelocity = Vector3.zero;
        currentMoveInput = Vector2.zero;
        
        if (stateMachine.playerInputManager != null)
        {
            stateMachine.playerInputManager.OnMoveEvent -= HandleMovementInput;
        }
    }
}