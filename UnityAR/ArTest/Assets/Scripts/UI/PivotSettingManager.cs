using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Singleton manager for the pivot-use toggle. When enabled, model rotation and
/// positioning use the object's transform origin as the pivot; when disabled the
/// bounds center is used instead. Must be initialised by calling
/// <see cref="Initialize"/> (done by <see cref="MiscOptionsManager"/>).
/// </summary>
public class PivotSettingManager : MonoBehaviour
{
    /// <summary>The single active instance of <see cref="PivotSettingManager"/>.</summary>
    public static PivotSettingManager Instance { get; private set; }

    /// <summary>
    /// Fired when the pivot-use preference changes.
    /// Parameter is <c>true</c> when the pivot should be used.
    /// </summary>
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
    
    /// <summary>
    /// Wires up the toggle listener and state-machine callbacks.
    /// Call once after all singleton instances are ready.
    /// </summary>
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

    /// <summary>
    /// Programmatically sets the pivot-use preference and fires
    /// <see cref="OnPivotUseChange"/>.
    /// </summary>
    /// <param name="value"><c>true</c> to use the transform pivot; <c>false</c> for bounds center.</param>
    public void SetUsePivot(bool value)
    {
        OnPivotUseChange?.Invoke(value);
    }

    
}
