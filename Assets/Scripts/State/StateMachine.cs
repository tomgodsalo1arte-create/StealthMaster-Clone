using Assets.Scripts.State;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; set; }

    public void Initialize(IState state)
    {

        CurrentState = state;
        state.Enter();
    }
    public void TransitionTo(IState next)
    {
        if (next == null) return;
        if (ReferenceEquals(next, CurrentState)) return;
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
    }
     public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }
}
