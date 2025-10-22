using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaneAreaCalculator : MonoBehaviour
{
    private ARPlaneManager planeManager;

    private void Awake()
    {
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }



    bool calculateArea(BoxCollider patentCollider)
    {

        foreach (ARPlane plane in planeManager.trackables)
        {
            
            
        }

        return false;
    }
}
