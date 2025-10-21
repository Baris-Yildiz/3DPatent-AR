using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdleState : IState
{
    GameObject m_QRButton;
    public IdleState()
    {
        m_QRButton = GameObject.FindWithTag("QRButton");
    }

    public void Enter()
    {
        m_QRButton.GetComponentInChildren<TextMeshProUGUI>().text = "QR Scan";
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        
    }
}
