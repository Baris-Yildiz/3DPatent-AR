using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PatentTransformer : MonoBehaviour
{
    [SerializeField] private ObjectSpawnerAR spawner;
    private GameObject patent;
    
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
    private void Awake()
    {
        spawner = FindFirstObjectByType<ObjectSpawnerAR>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (dragDeltaAction.action != null) dragDeltaAction.action.performed += onDragDelta;
        if (pinchDeltaAction.action != null) pinchDeltaAction.action.performed += onPinchDelta;
        //if (twistDeltaAction.action != null) twistDeltaAction.action.performed += onTwistDelta;
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
        float xDegrees = -delta.y * dragRotationSpeed;
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
        Transform targetTransform = spawner.getActivePatent().transform;
        if (targetTransform == null) return;
        if (Mathf.Abs(degreesY) >= Mathf.Abs(degreesX))
        {
            targetTransform.Rotate(Vector3.up, degreesY, Space.World);     
        }
        else
        {
            targetTransform.Rotate(Vector3.right , degreesX , Space.World);   
        }
    }
    void changeScale(float scale)
    {
        Transform targetTransform = spawner.getActivePatent().transform;
        if (targetTransform == null) return;
        Vector3 newScale = targetTransform.localScale * scale;
        
        float clampedX = Mathf.Clamp(newScale.x, minScale, maxScale);
        float clampedY = Mathf.Clamp(newScale.y, minScale, maxScale);
        float clampedZ = Mathf.Clamp(newScale.z, minScale, maxScale);

        targetTransform.localScale = new Vector3(clampedX, clampedY, clampedZ);
    }
}
