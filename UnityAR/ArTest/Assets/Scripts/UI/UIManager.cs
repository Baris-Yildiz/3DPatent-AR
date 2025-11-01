using System;

using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField]private PlaneDetectionController planeDetectionController;
    [SerializeField]private PlaneDetectionController screenFitController;
    [SerializeField]private ResetPatentButton _resetPatentButton;
    private RaycastHandler raycastHandler;
    private ObjectSpawnerAR _objectSpawnerAR;

    private void Awake()
    {
        raycastHandler = FindFirstObjectByType<RaycastHandler>();
        _objectSpawnerAR = FindFirstObjectByType<ObjectSpawnerAR>();
    }

    private void OnEnable()
    {
        planeDetectionController.planeDetectionToggleChange += HandlePlaneDetectionChange;
        screenFitController.planeDetectionToggleChange += HandleScreenFitDetectionChange;
        _resetPatentButton.resetPatentTransformAction += ResetPatentTransform;
    }

    private void OnDisable()
    {
        planeDetectionController.planeDetectionToggleChange -= HandlePlaneDetectionChange;
        screenFitController.planeDetectionToggleChange -= HandleScreenFitDetectionChange;
        _resetPatentButton.resetPatentTransformAction -= ResetPatentTransform;
    }

    void HandlePlaneDetectionChange(bool isOn)
    {
        Debug.Log("hello from canvasbaba");
        raycastHandler.changePlaneDetection(isOn);
    }

    void ResetPatentTransform()
    {
        _objectSpawnerAR.ResetPatentTransform();
    }

    void HandleScreenFitDetectionChange(bool isOn)
    {
        ScreenScaler.instance.SetScreenFit(isOn);
    }
}
