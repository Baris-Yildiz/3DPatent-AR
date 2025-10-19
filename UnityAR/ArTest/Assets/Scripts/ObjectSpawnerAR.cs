using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


enum PlacementMode
{
    Front,
    Center,
}

public class ObjectSpawnerAR : MonoBehaviour
{
    [SerializeField] private PlacementMode _placementMode = PlacementMode.Front;
    [SerializeField] private GameObject patent;
    [SerializeField] private float centerPlacementY = 1.1f;
    [SerializeField] private float frontPlacementZ = 2f;
    private GameObject activePatent;
    private void OnEnable()
    {
        RaycastHandler.clickEvent += SpawnObject;
    }

    private void OnDisable()
    {
        RaycastHandler.clickEvent -= SpawnObject;
    }
    
    void SpawnObject(List<ARRaycastHit> hits)
    {
        Debug.Log("trying to spawn");
        if (hits == null)
        {
            spawnWithNoRaycast();
        }
        else
        {
            Pose hitPose = hits[0].pose;
            spawnWithRaycast(hitPose);
        }
    }

    void spawnWithNoRaycast()
    {
        Vector3 position = Vector3.zero;
        Camera cam = Camera.main;
        switch (_placementMode)
        {
            case PlacementMode.Center:
                position = cam.transform.position;
                position.y -= centerPlacementY;
                break;
            case PlacementMode.Front:
                position = cam.transform.position + cam.transform.forward * frontPlacementZ;
                break;
        }

        if (activePatent == null)
        {
            activePatent = Instantiate(patent, position, patent.transform.rotation);
        }
        else
        {
            activePatent.transform.position = position;
        }



    }

    void spawnWithRaycast(Pose hitPose)
    {
        if (activePatent == null)
        {
            activePatent = Instantiate(patent, hitPose.position, patent.transform.rotation);
        }
        else
        {
            activePatent.transform.position = hitPose.position;
        }
    }

    public GameObject getActivePatent()
    {
        return activePatent;
    }
}
