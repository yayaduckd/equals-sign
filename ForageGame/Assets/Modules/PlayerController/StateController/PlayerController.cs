using UnityEngine;

public class PlayerController : StateController
{
    State idleState = new Idle();

    private void Awake()
    {
        DefaultState = idleState;
        ChangeState(DefaultState);
    }

    private void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }

    private class Idle : State
    {

    }
}