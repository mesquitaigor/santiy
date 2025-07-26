using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected State currentState;

    public void ChangeState(State newState)
    { 
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update(Time.deltaTime);
        }
    }
}
