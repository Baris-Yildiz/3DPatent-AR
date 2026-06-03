using System;
using UnityEngine;

public class PatentManager : MonoBehaviour
{
    public static PatentManager Instance;
    [SerializeField] private GameObject patent;
    private GameObject activePatent;
    public event Action patentDeletedEvent;
    public event Action patentSpawnedEvent;

    public Vector3 initialScale { get; private set; }
    public Quaternion initialRotation { get; private set; }

    public Vector3 initialPosition { get; private set; }

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

    public GameObject ActivePatent
    {

        
         set => activePatent = value;
            
         
        get => activePatent;
    }

    public GameObject Patent
    {
        set => patent = value;
        get => patent;
    }

    public void DeleteActivePatent()
    {
        Destroy(activePatent);
    }

    public void OnScanEnter()
    {
        Debug.Log("ualalalala5551515");
        DeleteActivePatent();
        Patent = null;
        ActivePatent = null;
        patentDeletedEvent?.Invoke();
    }

    public void SetInitialTransform()
    {
        initialRotation = activePatent.transform.rotation;
        initialScale = activePatent.transform.localScale;
        initialPosition = activePatent.transform.position;
    }

    public void InvokePatentSpawned()
    {
        patentSpawnedEvent?.Invoke();
    }
}
