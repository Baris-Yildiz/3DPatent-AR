using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModelTransformManager : MonoBehaviour
{
    public UIToggle LockModelUIToggle;
    public Button ResetModelTransformButton;
    public Button DeleteModelTransformButton;
    
    public static ModelTransformManager Instance { get; private set; }

    public event UnityAction<bool> OnLockModel;
    public event UnityAction OnResetModelTransform;
    public event UnityAction OnDeleteModelTransform;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LockModelUIToggle.OnToggleValueChanged += (bool isOn) => { OnLockModel?.Invoke(isOn); };
        ResetModelTransformButton.onClick.AddListener(() => { OnResetModelTransform?.Invoke(); });
        DeleteModelTransformButton.onClick.AddListener(() => { OnDeleteModelTransform?.Invoke(); });

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () => { LockModelUIToggle.GetComponent<Toggle>().isOn = false; };

        gameObject.SetActive(false);
    }
}
