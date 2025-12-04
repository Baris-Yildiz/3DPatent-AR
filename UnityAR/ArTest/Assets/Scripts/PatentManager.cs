using System;
using UnityEngine;

public class PatentManager : MonoBehaviour
{
    public static PatentManager Instance;
    [SerializeField] private GameObject patent;
    private GameObject activePatent;
    public event Action patentDeletedEvent;

    public Vector3 initialScale { get; private set; }
    public Quaternion initialRotation { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        StateMachine.Instance.GetState(StateMachine.States.NO_MODEL_VIEW_STATE).OnStateEnter += OnScanEnter;
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE).OnStateEnter += OnScanEnter;
    }

    public GameObject Patent
    {
        set => patent = value;
        get => patent;
    }
    
    public GameObject ActivePatent
    {

        
         set => activePatent = value;
            
         
        get => activePatent;
    }

    public void DeleteActivePatent()
    {
        Destroy(activePatent);
    }

    public void OnScanEnter()
    {
        DeleteActivePatent();
        patent = null;
        patentDeletedEvent?.Invoke();
    }

    public void SetInitialTransform()
    {
        initialRotation = activePatent.transform.rotation;
        initialScale = activePatent.transform.localScale;
    }

}
