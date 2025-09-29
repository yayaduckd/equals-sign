using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    Stack<State> stateStack = new Stack<State>();
    protected State currentState;
    protected State DefaultState;

    /// <summary>
    /// Exits the current state and enters the given one.
    /// newState should not be null.
    /// If currentState is null, only enters newState.
    /// If newState is the same as currentState, does nothing.
    /// </summary>
    /// <exception cref="System.NullReferenceException">Thrown when newState is null.</exception>"
    /// <param name="newState">The state to transition to.</param>
    protected void ChangeState(State newState)
    {
        if(currentState == newState) return; //Do nothing if the new state is the same as the current one
        if(newState == null) throw new System.NullReferenceException("newState cannot be null"); //Throw an error if newState is null

        if(currentState != null) { stateStack.Push(currentState); }
        TransitionToState(newState);
    }

    private void TransitionToState(State newState)
    {
        //Exit the current state
        if (currentState != null)
        {
            currentState.Exit();
        }

        //Enter the new state
        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// Reverts to the previous state in the stateStack. If the stack is empty, reverts to DefaultState.
    /// </summary>
    protected void RevertState()
    {
        //Go back to the previous state
        if (stateStack.Count > 0)
        {
            State previousState = stateStack.Pop();
            TransitionToState(previousState);
        }
        else
        {
            TransitionToState(DefaultState);
        }
    }
}