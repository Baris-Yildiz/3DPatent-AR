using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    private PlaneDetectionController planeDetectionController;
    private RaycastHandler raycastHandler;


    private void Awake()
    {
        planeDetectionController = GetComponentInChildren<PlaneDetectionController>();
        raycastHandler = FindFirstObjectByType<RaycastHandler>();
    }

    private void OnEnable()
    {
        planeDetectionController.planeDetectionToggleChange += HandlePlaneDetectionChange;
    }

    private void OnDisable()
    {
        planeDetectionController.planeDetectionToggleChange -= HandlePlaneDetectionChange;
    }

    void HandlePlaneDetectionChange(bool isOn)
    {
        Debug.Log("hello from canvasbaba");
        raycastHandler.changePlaneDetection(isOn);
    }
}
