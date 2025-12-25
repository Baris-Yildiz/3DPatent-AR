using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModelPlaneDetectionManager : MonoBehaviour
{
    GameObject m_Parent;

    public static ModelPlaneDetectionManager Instance { get; private set; }
    public event UnityAction<bool> OnPlaneDetectionChange;

    private UIToggle m_UIToggleComponent;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        m_UIToggleComponent = GetComponent<UIToggle>();

        m_UIToggleComponent.OnToggleValueChanged += (bool isOn) => { OnPlaneDetectionChange?.Invoke(isOn); };

        OnPlaneDetectionChange += (bool isOn) => { print("plane detection pressed"); };

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () => { GetComponent<Toggle>().isOn = false; };
    }
}
