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
    private bool isScreenFitOn = true;
    [SerializeField] [Range(0.1f, 1f)] private float scaleAmount = 0.8f;
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

    public void SetScreenFit(bool isOn)
    {
        isScreenFitOn = isOn;
    }
    

    // void Update()
    // {
    //     scaleAmount = slider.value;
    // }

    public float FitScreen(GameObject patent)
    {
        if (patent == null || cam == null || !isScreenFitOn)
        {
            Debug.Log("bir şeyler null kardeşim");
            return -1f;
        }
        // BoxCollider collider = patent.GetComponentInChildren<BoxCollider>();
        patent.transform.localScale = Vector3.one;
        Bounds bounds = PivotSetter.GetBounds(patent);
        if (originalCenter == Vector3.negativeInfinity && originalSize == Vector3.negativeInfinity)
        {
            originalCenter = bounds.center;
            originalSize = bounds.size;
        } 
        // Vector3 vectorToPatent = (bounds.min ) - cam.transform.position;
        //float projectedDistance = Math.Abs(Vector3.Dot(vectorToPatent, cam.transform.forward));
        float projectedDistance = Vector3.Distance(cam.transform.position, patent.transform.position);
        float verticalFOVRadians = cam.fieldOfView * Mathf.Deg2Rad;
        float viewHeightWorld = 2.0f * projectedDistance * Mathf.Tan(verticalFOVRadians * 0.5f);
        float viewWidthWorld = viewHeightWorld * cam.aspect;
        
       //txt.text ="Bounds center : " +  bounds.center.ToString() + " Bounds Min: " + bounds.min.ToString() + " View Height : " + viewHeightWorld;
       //txt.text = "Projected Distance : " + projectedDistance.ToString() + " View Height : " + viewHeightWorld + " Bounds: " + bounds.min.ToString();
        
        float scaleNeededX = (viewWidthWorld * scaleAmount) / Math.Max(bounds.size.z , bounds.size.x);
        float scaleNeededY = (viewHeightWorld * scaleAmount) / bounds.size.y;
        float smallestScale = Mathf.Min(scaleNeededX, scaleNeededY);
        return smallestScale;
    }
    
    
    
    
}
