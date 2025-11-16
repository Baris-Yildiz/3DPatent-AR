using System;
using UnityEngine;
using UnityEngine.UI;

public class AddModelManager : MonoBehaviour
{
    private GameObject m_AddModelPopupWindow;
    
    private Button m_AddModelButton;
    public Sprite CancelSprite;
    public Sprite AddSprite;
    private Image m_ButtonIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_AddModelButton = GetComponent<Button>();
        m_ButtonIcon = GetComponentsInChildren<Image>()[1];

        SetButtonToDisplayAddModelPopup();

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += SetButtonToDisplayAddModelPopup;

        StateMachine.Instance.GetState(StateMachine.States.NO_MODEL_VIEW_STATE)
            .OnStateEnter += SetButtonToDisplayAddModelPopup;

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateEnter += () => { m_AddModelButton.interactable = false; };

        m_AddModelPopupWindow = null;
    }

    private void SetButtonToDisplayAddModelPopup()
    {
        m_AddModelButton.interactable = true;
        m_ButtonIcon.sprite = AddSprite;
        m_AddModelButton.onClick.RemoveAllListeners(); 
        m_AddModelButton.onClick.AddListener(DisplayAddModelPopup);
    }

    private void SetButtonToCancelQRScan()
    {
        m_AddModelButton.interactable = true;
        m_ButtonIcon.sprite = CancelSprite;
        m_AddModelButton.onClick.RemoveAllListeners(); 
        m_AddModelButton.onClick.AddListener(() => { 
            StateMachine.Instance.SetState(StateMachine.States.NO_MODEL_VIEW_STATE);
            QRScanner.Instance.StopScanner();
        });   
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
        SetButtonToCancelQRScan();
    }

    private void OnAddModelPopupCancel()
    {
        m_AddModelButton.interactable = true;
        m_AddModelPopupWindow.SetActive(false);
    }
}
