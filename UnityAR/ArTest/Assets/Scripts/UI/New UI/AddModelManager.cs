using System;
using UnityEngine;
using UnityEngine.UI;

public class AddModelManager : MonoBehaviour
{
    private GameObject m_AddModelPopupWindow;
    
    private Button m_AddModelButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_AddModelButton = GetComponent<Button>();
        m_AddModelButton.onClick.AddListener(DisplayAddModelPopup);

        //TODO: Instead of interactable=false, can convert to a "cancel qr" button
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => { m_AddModelButton.interactable = false; };
        
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateExit += () => { m_AddModelButton.interactable = true; };

        m_AddModelPopupWindow = null;
    }

    public void DisplayAddModelPopup()
    {
        m_AddModelButton.interactable = false;

        if (m_AddModelPopupWindow != null)
        {
            m_AddModelPopupWindow.SetActive(true);
            return;
        }

        PopupBuilder builder = PopupBuilder.Create();

        m_AddModelPopupWindow = builder
            .WithContentText("Yeni bir model yerleþtirmek istiyor musunuz?")
            .WithContentText("Modelin QR kodunu okutmaya yönlendirilecceksiniz.")
            .WithHeader("BÝLGÝ")
            .WithFooterButton("QR Kod Okut", PopupWindow.BUTTON_COLOR_OK, OnAddModelPopupOK)
            .WithFooterButton("Ýptal Et", PopupWindow.BUTTON_COLOR_CANCEL, OnAddModelPopupCancel)
            .Get();
    }

    private void OnAddModelPopupOK()
    {
        StateMachine.Instance.SetState(StateMachine.States.QR_SCAN_STATE);
        m_AddModelPopupWindow.SetActive(false);
    }

    private void OnAddModelPopupCancel()
    {
        m_AddModelButton.interactable = true;
        m_AddModelPopupWindow.SetActive(false);
    }
}
