using System;
using System.Collections.Generic;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; } 
    
    [SerializeField] private int poolSize = 5;
    [SerializeField] private LogText logPrefab;
    
    private Queue<LogText> logPool;
    private List<LogText> activeLogs;
    
    
    [SerializeField] private float logDistanceY = 0.5f;
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;
    
    //[SerializeField] private int maxTextSize = 30;
   
    
    
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

        logPool = new Queue<LogText>(poolSize);
        activeLogs = new List<LogText>(poolSize);
    }

    private void InitializeLogPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            LogText log = Instantiate(logPrefab, transform);
            log.gameObject.SetActive(false);
            logPool.Enqueue(log);
        }
    }

    public void EnqueueLog(LogText log)
    {
        if (log == null)
        {   
            Debug.Log("Log is null");
            return;
        }
        log.gameObject.SetActive(false);
        activeLogs.Remove(log);
        logPool.Enqueue(log);
    }

    public void DequeueLog(String logText)
    {
        LogText log = logPool.Dequeue();
        log.gameObject.SetActive(true);
        log.StartLog(startPoint , new Vector2(endPoint.x , endPoint.y + (logDistanceY*activeLogs.Count)));
        activeLogs.Add(log);
    }
    

    private void UpdateLogs()
    {
        for (int i = 0; i < activeLogs.Count; i++)
        {
            activeLogs[i].UpdateLog();
        }
    }

    void Update()
    {
        UpdateLogs();   
    }
}
