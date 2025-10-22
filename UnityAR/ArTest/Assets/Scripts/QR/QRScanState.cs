using TMPro;
using UnityEngine;

public class QRScanState : IState
{
    GameObject m_QRButton;
    QRScanner m_QRScanner;
   

    public QRScanState()
    {
        m_QRButton = GameObject.FindWithTag("QRButton");
        m_QRScanner = GameObject.FindWithTag("QRScanner").GetComponent<QRScanner>();
        
    }

    public void Enter()
    {
        m_QRButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop QR Scan";
        m_QRScanner.ResetScanner();
        m_QRScanner.ScanScreen();
    }

    public void Exit()
    {
        
    }

    public void Update()
    {

    }
}
