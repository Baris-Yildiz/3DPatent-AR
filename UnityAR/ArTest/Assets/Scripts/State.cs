using System;
using UnityEngine;

public class State
{
    public event Action OnStateEnter;
    public event Action OnStateUpdate;
    public event Action OnStateExit;

    public void Enter()
    {
        OnStateEnter?.Invoke(); 
    }

    public void Exit()
    {
        OnStateExit?.Invoke();
    }

    public void Update()
    {
        OnStateUpdate?.Invoke();
    }
}
