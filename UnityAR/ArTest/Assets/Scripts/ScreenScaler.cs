using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public  class ScreenScaler : MonoBehaviour
{
    public static ScreenScaler instance;
    public TextMeshProUGUI txt;
    private Camera cam;
    private Vector3 originalSize = Vector3.negativeInfinity;
    private Vector3 originalCenter = Vector3.negativeInfinity;
    [SerializeField] [Range(0.1f, 1f)] private float scaleAmount;
    [SerializeField] private Slider slider;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            cam = Camera.main;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
    }


    void Update()
    {
        scaleAmount = slider.value;
    }

    public float FitScreen(GameObject patent)
    {
        if (patent == null || cam == null)
        {
            Debug.Log("bir şeyler null kardeşim");
            return -1;
        }
        BoxCollider collider = patent.GetComponentInChildren<BoxCollider>();
        Bounds bounds = collider.bounds;
        if (originalCenter == Vector3.negativeInfinity && originalSize == Vector3.negativeInfinity)
        {
            originalCenter = bounds.center;
            originalSize = bounds.size;
        }

        
        Vector3 vectorToPatent = (bounds.center ) - cam.transform.position;
        float projectedDistance = Math.Abs(Vector3.Dot(vectorToPatent, cam.transform.forward));
       // float targetDistance = Vector3.Distance(cam.transform.position, bounds.center);
        float verticalFOVRadians = cam.fieldOfView * Mathf.Deg2Rad;
        float viewHeightWorld = 2.0f * projectedDistance * Mathf.Tan(verticalFOVRadians * 0.5f);
        float viewWidthWorld = viewHeightWorld * cam.aspect;
        
       //txt.text ="Bounds center : " +  bounds.center.ToString() + " Bounds Min: " + bounds.min.ToString() + " View Height : " + viewHeightWorld;
       txt.text = "Projected Distance : " + projectedDistance.ToString() + " View Height : " + viewHeightWorld;
        
        float scaleNeededX = (viewWidthWorld * scaleAmount) / Mathf.Max(bounds.size.x , bounds.size.y);
        float scaleNeededY = (viewHeightWorld * scaleAmount) / bounds.size.y;
        
        float smallestScale = Mathf.Min(scaleNeededX, scaleNeededY);
        
        return smallestScale;
    }
    
    
}
