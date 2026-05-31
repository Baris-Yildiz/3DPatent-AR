using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PivotSettingManager : MonoBehaviour
{
   

    public static PivotSettingManager Instance { get; private set; }
    public event UnityAction<bool> OnPivotUseChange;

    private UIToggle m_UIToggleComponent;

    private Toggle pivotToggle;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
    }
    
    public void Initialize()
    {
        pivotToggle = GetComponent<Toggle>();
        m_UIToggleComponent = GetComponent<UIToggle>();

        m_UIToggleComponent.OnToggleValueChanged += (bool isOn) =>
        {
            OnPivotUseChange?.Invoke(isOn);

        };

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () => { pivotToggle.isOn = true; };
    }

    public void SetUsePivot(bool value)
    {
        OnPivotUseChange?.Invoke(value);
    }

    
}
