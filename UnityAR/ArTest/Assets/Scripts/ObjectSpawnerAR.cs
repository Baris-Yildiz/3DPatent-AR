using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

enum PlacementMode
{
    Front,
    Center,
}

public class ObjectSpawnerAR : MonoBehaviour
{
    [SerializeField] private PlacementMode _placementMode = PlacementMode.Front;
    [SerializeField]private GameObject patent;
    [SerializeField] private float centerPlacementY = 1.1f;
    [SerializeField] private float frontPlacementZ = 2f;
    [SerializeField]private TextMeshProUGUI scaleText;
    [SerializeField]private TextMeshProUGUI positionText;
    public static event Action<bool> objectSpawnedEvent; 
    private GameObject activePatent;
    public bool spawnWithinScreen = true;



  
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
        if (PatentManager.Instance.Patent == null || PatentManager.Instance.ActivePatent != null) return;
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

        if (PatentManager.Instance.ActivePatent == null)
        {
            PatentManager.Instance.ActivePatent = Instantiate(PatentManager.Instance.Patent, position, PatentManager.Instance.Patent.transform.rotation);
        }
        // else
        // {
        //     activePatent.transform.position = position;
        //    
        // }
        objectSpawnedEvent?.Invoke(false);
        PatentManager.Instance.SetInitialTransform();
      //  positionText.text = "X: " + activePatent.transform.position.x.ToString() + " Y: " + activePatent.transform.position.y.ToString() + " Z : " + activePatent.transform.position.z.ToString();


    }

    void spawnWithRaycast(Pose hitPose)
    {
        if (PatentManager.Instance.ActivePatent == null)
        {
            PatentManager.Instance.ActivePatent = Instantiate(PatentManager.Instance.Patent, hitPose.position, PatentManager.Instance.Patent.transform.rotation);
            
        }
        // else
        // {
        //     activePatent.transform.position = hitPose.position;
        //     
        // }
        objectSpawnedEvent?.Invoke(true);
        PatentManager.Instance.SetInitialTransform();
     //   positionText.text = "X: " + activePatent.transform.position.x.ToString() + " Y: " + activePatent.transform.position.y.ToString() + " Z : " + activePatent.transform.position.z.ToString();
    }

    private void FitScreen()
    {
        if (!spawnWithinScreen) return;
        float scaleAmount = ScreenScaler.instance.FitScreen(activePatent);
        activePatent.transform.localScale *= scaleAmount;
        scaleText.text ="Scale:          " + activePatent.transform.localScale.y.ToString()  ;
        Debug.Log(scaleAmount);
    }

    public void SetActivePatent(GameObject downloaded_patent)
    {
        patent = downloaded_patent;
        Debug.Log("settedddd");
    }
    
    public GameObject getActivePatent()
    {
        return activePatent;
    }

    public void ResetPatentTransform()
    {
        if (patent == null) return;
        Camera cam = Camera.main;
        patent.transform.localScale = Vector3.one;
        activePatent.transform.localScale = Vector3.one;
        Vector3 offSet = PivotSetter.ChangeToOneOneMode(activePatent,cam.transform,1f);
    }
    
    
    
    
}
