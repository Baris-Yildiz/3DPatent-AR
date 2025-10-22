using TMPro;
using UnityEngine;

public class QRScanCompleteState : IState
{
    GameObject m_QRButton;
    QRNetworkHandler m_QRNetworkHandler;

    public QRScanCompleteState()
    {
        m_QRButton = GameObject.FindWithTag("QRButton");
        m_QRNetworkHandler = GameObject.FindWithTag("QRNetworkHandler").GetComponent<QRNetworkHandler>();
    }

    public void Enter()
    {
        m_QRButton.GetComponentInChildren<TextMeshProUGUI>().text = "QR Scan Complete!";
        m_QRNetworkHandler.StartDownloadingModel();
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        
    }
}
