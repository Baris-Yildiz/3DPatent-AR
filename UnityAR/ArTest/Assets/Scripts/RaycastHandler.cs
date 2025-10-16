using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private void OnEnable()
    {
        
        tapAction.action.performed += CheckInput;
    }

    private void OnDisable()
    {
        tapAction.action.performed -= CheckInput;
        tapAction.action.Disable();
        if (positionAction.action != null)
        {
            positionAction.action.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("gagagaga");
        HandleClick();
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
         text.text = "Clicked Amount: " + touchCount.ToString();
        if (positionAction.action != null)
        {
            touchPosition = positionAction.action.ReadValue<Vector2>();
        }
        else
        {
            touchPosition = Mouse.current.position.ReadValue();
        }

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

}
