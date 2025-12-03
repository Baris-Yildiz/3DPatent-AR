using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RaycastHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField]private InputActionProperty tapAction;      
    [SerializeField]private InputActionProperty positionAction;
    [SerializeField] private bool usePlaneDetection = true;
    [SerializeField] private bool showDetectedPlanes = false;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    public static event Action<List<ARRaycastHit>> clickEvent;
    private Vector2 touchPosition;
    private bool touched = false;
    private int touchCount = 0;
    private void Awake()
    {
       
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        planeManager.enabled = showDetectedPlanes;

    }

    private void Start()
    {
        ScalingModeController.Instance.modeChanged += OnModeChange;
        ModelPlaneDetectionManager.Instance.OnPlaneDetectionChange += changePlaneDetection;
    }
    
    private void OnEnable()
    {
        
        tapAction.action.performed += CheckInput;
    }

    private void OnDisable()
    {
        tapAction.action.performed -= CheckInput;
    }

    public void OnModeChange(bool isOneToOne)
    {
        changePlaneDetection(!isOneToOne);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("gagagaga");
        HandleClick();
       // text.text = Camera.main.transform.position.ToString();
    }

    void HandleClick()
    {
        if (touched)
        {
            touched = false;
            Debug.Log("Clicked");
           RaiseClickEvent();
        }
    }

     void CheckInput(InputAction.CallbackContext ctx)
     {
           
         touchCount++;
         int pointerId = -1;
         if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
         {
             pointerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
             
         }
         // if (EventSystem.current.IsPointerOverGameObject(pointerId))
         // {
         //     //text.text = "Clicked on UI " + touchCount.ToString();
         //     return;
         // }
        // text.text = "Clicked Amount: " + touchCount.ToString();
        if (positionAction.action != null)
        {
            touchPosition = positionAction.action.ReadValue<Vector2>();
        }
        else
        {
            touchPosition = Mouse.current.position.ReadValue();
        }

        //text.text = "Clicked on gameobject " + touchCount.ToString();
        touched = true;

    }

    void RaiseClickEvent()
    {
        if (usePlaneDetection && raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Debug.Log("plane detected raycast");
            clickEvent?.Invoke(hits);
        }
        else if(!usePlaneDetection)
        {
            Debug.Log("non plane detection raycast");
            clickEvent?.Invoke(null);
        }
    }

    public void changePlaneDetection(bool isOn)
    {
        if (isOn == usePlaneDetection) return;
        
        usePlaneDetection = isOn;
        if (isOn)
        {
            planeManager.subsystem?.Start();
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }
        else
        {
            planeManager.requestedDetectionMode = PlaneDetectionMode.None;
        }
        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(isOn);
                    
        }
    }

   

}
