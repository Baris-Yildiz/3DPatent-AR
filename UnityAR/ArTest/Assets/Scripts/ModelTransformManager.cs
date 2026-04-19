using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModelTransformManager : MonoBehaviour
{
    public UIToggle LockModelUIToggle;
    public Button ResetModelTransformButton;
    public Button DeleteModelTransformButton;
    public Toggle LockModelTransformToggle;
    public static ModelTransformManager Instance { get; private set; }

    public event UnityAction<bool> OnLockModel;
    public event UnityAction OnResetModelTransform;
    public event UnityAction OnDeleteModel;


    private GameObject m_ResetTransformPopupWindow;
    private GameObject m_DeleteModelPopupWindow;

    private void Awake()
    {
        m_ResetTransformPopupWindow = null;
        m_DeleteModelPopupWindow = null;
        Instance = this;
    }

    void Start()
    {
        
        LockModelUIToggle.OnToggleValueChanged += (bool isOn) => { OnLockModel?.Invoke(isOn); };
        ResetModelTransformButton.onClick.AddListener(DisplayResetTransformPopup);
        DeleteModelTransformButton.onClick.AddListener(DisplayDeleteModelPopup);

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () => { LockModelUIToggle.GetComponent<Toggle>().isOn = false; };
        ScalingModeController.Instance.modeChanged += LockLockToggle;
        ScalingModeController.Instance.modeChanged += LockResetButton;

        gameObject.SetActive(false);
    }

    private void LockLockToggle(bool isOn)
    {
        LockModelTransformToggle.isOn = isOn;
        LockModelTransformToggle.interactable = !isOn;
    }

    private void LockResetButton(bool isOn)
    {
        ResetModelTransformButton.interactable = !isOn;
    }

    private void DisplayResetTransformPopup()
    {
        ResetModelTransformButton.interactable = false;

        if (m_ResetTransformPopupWindow != null)
        {
            m_ResetTransformPopupWindow.SetActive(true);
            return;
        }

        PopupBuilder builder = PopupBuilder.Create();

        m_ResetTransformPopupWindow = builder
            .WithContentText("This will reset your model back to the original scale.")
            .WithHeader("WARNING")
            .WithFooterButton("OK", PopupWindow.BUTTON_COLOR_OK, OnResetTransformPopupOK)
            .WithFooterButton("Cancel", PopupWindow.BUTTON_COLOR_CANCEL, OnResetTransformPopupCancel)
            .Get();
    }

    private void OnResetTransformPopupOK()
    {
        ResetModelTransformButton.interactable = true;
        m_ResetTransformPopupWindow.SetActive(false);
        OnResetModelTransform?.Invoke();
    }

    private void OnResetTransformPopupCancel()
    {
        ResetModelTransformButton.interactable = true;
        m_ResetTransformPopupWindow.SetActive(false);
    }


    private void DisplayDeleteModelPopup()
    {
        DeleteModelTransformButton.interactable = false;

        if (m_DeleteModelPopupWindow != null)
        {
            m_DeleteModelPopupWindow.SetActive(true);
            return;
        }

        PopupBuilder builder = PopupBuilder.Create();

        m_DeleteModelPopupWindow = builder
            .WithContentText("Your model will be deleted.")
            .WithHeader("WARNING")
            .WithFooterButton("OK", PopupWindow.BUTTON_COLOR_OK, OnDeleteModelPopupOK)
            .WithFooterButton("Cancel", PopupWindow.BUTTON_COLOR_CANCEL, OnDeleteModelPopupCancel)
            .Get();
    }

    private void OnDeleteModelPopupOK()
    {
        DeleteModelTransformButton.interactable = true;
        m_DeleteModelPopupWindow.SetActive(false);
        PatentManager.Instance.DeleteActivePatent();
        OnDeleteModel?.Invoke();
    }

    private void OnDeleteModelPopupCancel()
    {
        DeleteModelTransformButton.interactable = true;
        m_DeleteModelPopupWindow.SetActive(false);
    }
}
