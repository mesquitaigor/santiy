public abstract class PlayerMoveBaseState : State
{
    protected PlayerMoveStateMachine stateMachine;
    public PlayerMoveBaseState(PlayerMoveStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}