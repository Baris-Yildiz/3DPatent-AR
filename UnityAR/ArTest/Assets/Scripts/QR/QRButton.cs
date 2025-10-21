using UnityEngine;

public class QRButton : MonoBehaviour
{
    StateMachine m_StateMachine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_StateMachine = GameObject.FindWithTag("StateMachine").GetComponent<StateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnQRButtonClicked()
    {
        if (m_StateMachine.CurrentStateName == StateMachine.States.IDLE_STATE) //enter qr scanning mode
        {
            m_StateMachine.SetState(StateMachine.States.QR_SCAN_STATE);
        } else if (m_StateMachine.CurrentStateName == StateMachine.States.QR_SCAN_STATE) //quit qr scanning
        {
            m_StateMachine.SetState(StateMachine.States.IDLE_STATE);
        }
        
    }


}
