using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    public static StateMachine Instance
    {
        get; private set;
    }

    public enum States
    {
        IDLE_STATE,
        QR_SCAN_STATE,
        QR_SCAN_COMPLETE_STATE,
    }

    private Dictionary<States, State> m_StateMap = new();

    private State m_CurrentState;

    public States CurrentStateName;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

        m_StateMap[States.IDLE_STATE] = new State();
        m_StateMap[States.QR_SCAN_STATE] = new State();
        m_StateMap[States.QR_SCAN_COMPLETE_STATE] = new State();

        SetState(States.IDLE_STATE);
        
    }

    public void SetState(States stateName)
    {
        m_CurrentState?.Exit();

        m_CurrentState = m_StateMap[stateName];
        CurrentStateName = stateName;

        m_CurrentState.Enter();
    }

    public State GetState(States stateName)
    {
        return m_StateMap[stateName];
    }

    void Update()
    {
        m_CurrentState.Update();
    }
}
