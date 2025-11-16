using System;
using UnityEngine;
using UnityEngine.UI;

public class ModelViewManager : MonoBehaviour
{
    public UIToggle NormalViewUIToggle;
    public UIToggle OneToOneViewUIToggle;

    private Toggle NormalViewToggle;
    private Toggle OneToOneViewToggle;


    public static ModelViewManager Instance { get; private set; }

    public event Action OnNormalViewToggle;
    public event Action OnOneToOneViewToggle;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NormalViewToggle = NormalViewUIToggle.gameObject.GetComponent<Toggle>();
        OneToOneViewToggle = OneToOneViewUIToggle.gameObject.GetComponent<Toggle>();

        NormalViewUIToggle.OnToggleValueChanged += (bool isOn) => { SetNormalView(isOn); };
        OneToOneViewUIToggle.OnToggleValueChanged += (bool isOn) => { SetOneToOneView(isOn); };

        OnNormalViewToggle += () => { print("switch normal view"); };
        OnOneToOneViewToggle += () => { print("switch 11 view"); };

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () => { NormalViewToggle.isOn = true; };

        gameObject.SetActive(false);
    }

    private void SetNormalView(bool isOn)
    {
        if (isOn)
        {
            OneToOneViewToggle.isOn = false;
            OnNormalViewToggle?.Invoke();
            NormalViewToggle.interactable = false;
            OneToOneViewToggle.interactable = true;
        } 
    }

    private void SetOneToOneView(bool isOn)
    {
        if (isOn)
        {
            NormalViewToggle.isOn = false;
            OnOneToOneViewToggle?.Invoke();
            OneToOneViewToggle.interactable = false;
            NormalViewToggle.interactable = true;
        }
    }
}
