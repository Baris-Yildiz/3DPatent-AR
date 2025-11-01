using System;
using UnityEngine;


enum ArMode
{
    Patent,
    Qr
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [SerializeField] private GameObject qrScene;
    
    [SerializeField] private GameObject patentScene;
    private RaycastHandler raycastHandler;
    private ArMode currMode = ArMode.Qr;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
        
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        raycastHandler = FindAnyObjectByType<RaycastHandler>();
        ActivateQrParts();
    }

    public void ChangeMode()
    {
        if (currMode == ArMode.Patent)
        {
            ActivateQrParts();
        }
        else if (currMode == ArMode.Qr)
        {
            ActivatePatentParts();
        }
    }

    private void ActivateQrParts()
    {
        patentScene.SetActive(false);
        setPatentScripts(false);
        qrScene.SetActive(true);
        currMode = ArMode.Qr;
    }

    private void ActivatePatentParts()
    {
        qrScene.SetActive(false);
        setPatentScripts(true);
        patentScene.SetActive(true);
        currMode = ArMode.Patent;
    }

    private void setPatentScripts(bool active)
    {
        raycastHandler.changePlaneDetection(active);
        raycastHandler.enabled = active;
        
    }

}
