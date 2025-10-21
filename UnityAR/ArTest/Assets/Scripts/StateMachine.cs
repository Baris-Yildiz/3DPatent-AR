using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum States
    {
        IDLE_STATE,
        QR_SCAN_STATE,
        QR_SCAN_COMPLETE_STATE
    }

    private Dictionary<States, IState> m_StateMap = new();

    private IState m_CurrentState;

    public States CurrentStateName;

    void Start()
    {
        m_StateMap[States.IDLE_STATE] = new IdleState();
        m_StateMap[States.QR_SCAN_STATE] = new QRScanState();
        m_StateMap[States.QR_SCAN_COMPLETE_STATE] = new QRScanCompleteState();

        SetState(States.IDLE_STATE);
        
    }

    public void SetState(States stateName)
    {
        m_CurrentState?.Exit();

        m_CurrentState = m_StateMap[stateName];
        CurrentStateName = stateName;

        m_CurrentState.Enter();

    }

    void Update()
    {

        m_CurrentState.Update();
    }
}
