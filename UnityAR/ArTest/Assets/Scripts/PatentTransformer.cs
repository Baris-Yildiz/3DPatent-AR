using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PatentTransformer : MonoBehaviour
{
   // [SerializeField] private ObjectSpawnerAR spawner;
    private PatentManager patentManager;
    [SerializeField]private InputActionProperty dragDeltaAction;   
    [SerializeField]private InputActionProperty pinchDeltaAction;  
    [SerializeField]private InputActionProperty twistDeltaAction;  
    
    [SerializeField]private float dragRotationSpeed = 0.25f;
    [SerializeField]private float twistRotationSpeed = 1f;
    
    [SerializeField]private float pinchScaleSpeed = 0.01f;
    [SerializeField]private float minScale = 0.05f;
    [SerializeField]private float maxScale = 3f;
    
    [SerializeField]private bool useMouseFallback = true;
    [SerializeField]private float mouseDragToRotate = 0.2f;   // degrees per pixel
    [SerializeField]private float mouseScrollToScale = 0.1f;  // scale factor per scroll delta
    

   
    
    // internal state
    Vector3 initialScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        patentManager = PatentManager.Instance;
        if (ScalingModeController.Instance != null)
        {
            ScalingModeController.Instance.modeChanged += OnModeChange;    
        }

        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform += ResetRotation;    
        }

        ObjectSpawnerAR.objectSpawnedEvent += HandleSpawnPosition;
        
        // spawner = FindFirstObjectByType<ObjectSpawnerAR>();
    }

    // Update is called once per frame
    

    private void OnEnable()
    {
        if (dragDeltaAction.action != null) dragDeltaAction.action.performed += onDragDelta;
        if (pinchDeltaAction.action != null) pinchDeltaAction.action.performed += onPinchDelta;
        ObjectSpawnerAR.objectSpawnedEvent += HandleSpawnPosition;
        //if (twistDeltaAction.action != null) twistDeltaAction.action.performed += onTwistDelta;
    }

    public void OnModeChange(bool isOneToOne)
    {
        if (isOneToOne)
        {
            OneToOneScale();
        }
        else
        {
            
        }
    }

    private void OnDisable()
    {
        if (dragDeltaAction.action != null)
        {
            dragDeltaAction.action.performed -= onDragDelta;
         
        }
        if (pinchDeltaAction.action != null)
        {
            pinchDeltaAction.action.performed -= onPinchDelta;
            
        }
        ObjectSpawnerAR.objectSpawnedEvent -= HandleSpawnPosition;
        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform -= ResetRotation;
        }

       
        // if (twistDeltaAction.action != null)
        // {
        //     twistDeltaAction.action.performed -= onTwistDelta;
        //     
        // }
    }

    void onDragDelta(InputAction.CallbackContext ctx)
    {
        Vector2 delta = ctx.ReadValue<Vector2>();
        float yDegrees = -delta.x * dragRotationSpeed; // negative so drag-right rotates right (tweak if needed)
        float xDegrees = delta.y * dragRotationSpeed;
        rotateAroundUp(yDegrees , xDegrees);
        
    }

    void onPinchDelta(InputAction.CallbackContext ctx)
    {
        float pinchDelta = ctx.ReadValue<float>();
        float scaleFactor = 1f + pinchDelta * pinchScaleSpeed;
        changeScale(scaleFactor);
    }

    // void onTwistDelta(InputAction.CallbackContext ctx)
    // {
    //     float twistDelta = ctx.ReadValue<float>();
    //     float yDegrees = twistDelta * twistRotationSpeed;
    //     rotateAroundUp(yDegrees);
    // }
    
    void rotateAroundUp(float degreesY , float degreesX)
    {
        if (patentManager.ActivePatent == null) return;
        Transform targetTransform = patentManager.ActivePatent.transform;
        if (targetTransform == null) return;

      
        Vector3 centerBefore = PivotSetter.GetBounds(patentManager.ActivePatent).center;

    
        if (Mathf.Abs(degreesY) > 0.001f)
        {
            targetTransform.Rotate(Vector3.up, degreesY, Space.World);
        }
        if (Mathf.Abs(degreesX) > 0.001f)
        {
            targetTransform.Rotate(Camera.main.transform.right, degreesX, Space.World);
        }


        Physics.SyncTransforms();

        
        Vector3 centerAfter = PivotSetter.GetBounds(patentManager.ActivePatent).center;
        
        Vector3 drift = centerAfter - centerBefore;
        targetTransform.position -= drift;
    }
    void changeScale(float scale)
    {
        if (patentManager.ActivePatent == null) return;
        Transform targetTransform = patentManager.ActivePatent.transform;
        if (targetTransform == null) return;
        Vector3 newScale = targetTransform.localScale * scale;
        
        float clampedX = Mathf.Clamp(newScale.x, minScale, maxScale);
        float clampedY = Mathf.Clamp(newScale.y, minScale, maxScale);
        float clampedZ = Mathf.Clamp(newScale.z, minScale, maxScale);

        targetTransform.localScale = new Vector3(clampedX, clampedY, clampedZ);
    }

    private void Update()
    {
        // if (patentManager.ActivePatent != null)
        // {
        //     Bounds b = PivotSetter.GetBounds(patentManager.ActivePatent);
        //     patentManager.ActivePatent.transform.RotateAround(b.center , Vector3.up , 20*Time.deltaTime);
        // }
    }

    public void HandleSpawnPosition(bool toGround)
    {
        Debug.Log("ulalala");
        GameObject activePatent = patentManager.ActivePatent;
        if (activePatent == null) return;
        Debug.Log("lalalalal");
        float scale = ScreenScaler.instance.FitScreen(patentManager.ActivePatent);
        Transform t = activePatent.transform;
        if (scale > 0)
        {
            t.localScale *= scale;
        }
        PivotSetter.SnapToYOffset(activePatent , toGround);
        
    }

    public void ResetRotation()
    {
        GameObject activaPatent = patentManager.ActivePatent;
        if (activaPatent == null) return;
        activaPatent.transform.rotation = patentManager.initialRotation;
        activaPatent.transform.localScale = patentManager.initialScale;

    }

    public void OneToOneScale()
    {
        
        patentManager.Patent.transform.localScale = Vector3.one;
        patentManager.ActivePatent.transform.localScale = Vector3.one;
       // patentManager.ActivePatent.transform.rotation = Quaternion.identity;
       patentManager.ActivePatent.transform.rotation = patentManager.initialRotation;
       patentManager.Patent.transform.rotation = patentManager.initialRotation;
        PivotSetter.ChangeToOneOneMode(patentManager.ActivePatent, Camera.main.transform, 1);
        

    }
}
