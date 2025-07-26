using UnityEngine;

public abstract class State
{
    public abstract void Enter();
    public abstract void Update(float deltaTime);
    public abstract void Exit();
}
