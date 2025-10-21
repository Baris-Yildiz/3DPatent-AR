using TMPro;
using UnityEngine;

public class QRScanState : IState
{
    GameObject m_QRButton;
    QRScanner m_QRScanner;
    StateMachine m_StateMachine;

    public QRScanState()
    {
        m_QRButton = GameObject.FindWithTag("QRButton");
        m_QRScanner = GameObject.FindWithTag("QRScanner").GetComponent<QRScanner>();
        m_StateMachine = GameObject.FindWithTag("StateMachine").GetComponent<StateMachine>();
    }

    public void Enter()
    {
        m_QRButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop QR Scan";
        m_QRScanner.ResetScanner();
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        m_QRScanner.ScanScreen();
        if (!string.IsNullOrEmpty(m_QRScanner.QrCode))
        {
            m_StateMachine.SetState(StateMachine.States.QR_SCAN_COMPLETE_STATE);
        }

    }
}
