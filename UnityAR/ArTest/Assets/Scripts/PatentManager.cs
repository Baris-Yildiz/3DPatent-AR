using System;
using UnityEngine;

public class PatentManager : MonoBehaviour
{
    public static PatentManager Instance;
    [SerializeField] private GameObject patent;
    private GameObject activePatent;
    public static event Action patentDeletedEvent;

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

    public GameObject Patent
    {
        set => patent = value;
        get => patent;
    }
    
    public GameObject ActivePatent
    {
        set =>  activePatent = value;
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



}
