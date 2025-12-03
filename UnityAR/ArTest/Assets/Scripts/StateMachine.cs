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
        NO_MODEL_VIEW_STATE,
        QR_SCAN_STATE,
        QR_SCAN_COMPLETE_STATE,
        MODEL_VIEW_STATE
    }

    private Dictionary<States, State> m_StateMap = new();

    private State m_CurrentState;

    public States CurrentStateName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(this);
        }
       
        m_StateMap[States.NO_MODEL_VIEW_STATE] = new State();
        m_StateMap[States.QR_SCAN_STATE] = new State();
        m_StateMap[States.QR_SCAN_COMPLETE_STATE] = new State();
        m_StateMap[States.MODEL_VIEW_STATE] = new State();

        SetState(States.NO_MODEL_VIEW_STATE);
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
