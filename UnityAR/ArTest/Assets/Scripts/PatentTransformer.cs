using UnityEngine;
using UnityEngine.InputSystem;

public class PatentTransformer : MonoBehaviour
{
    private PatentManager patentManager;
    [SerializeField] private InputActionProperty dragDeltaAction;   
    [SerializeField] private InputActionProperty pinchDeltaAction;  
    [SerializeField] private InputActionProperty twistDeltaAction;  
    
    [SerializeField] private float dragRotationSpeed = 0.12f;
    [SerializeField] private float twistRotationSpeed = 0.5f;
    
    [SerializeField] private float pinchScaleSpeed = 0.05f;
    [SerializeField] private float minScale = 0.01f;
    [SerializeField] private float maxScale = 1f;
    
    [SerializeField] private bool useMouseFallback = true;
    [SerializeField] private float mouseDragToRotate = 0.2f;   
    [SerializeField] private float mouseScrollToScale = 0.1f;  

    [SerializeField] private float smoothSpeed = 2f;

    private Quaternion targetRotation;
    private Vector3 targetScale;
    private GameObject visualPivot;

    private void Start()
    {
        patentManager = PatentManager.Instance;
        ObjectSpawnerAR.objectSpawnedEvent += HandleSpawnPosition;
    }

    private void Update()
    {
        if (patentManager.ActivePatent == null) return;

        Transform t = patentManager.ActivePatent.transform;
        
        t.localScale = Vector3.Lerp(t.localScale, targetScale, Time.deltaTime * smoothSpeed);

        if (Quaternion.Angle(t.rotation, targetRotation) > 0.01f)
        {
            Bounds localBounds = PivotSetter.GetBounds(patentManager.ActivePatent);
            Vector3 worldCenter = t.TransformPoint(localBounds.center);

            Quaternion nextRotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * smoothSpeed);
            Quaternion diff = nextRotation * Quaternion.Inverse(t.rotation);
            
            diff.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 0f)
            {
                if (angle > 180) angle -= 360; 
                t.RotateAround(worldCenter, axis, angle);
            }
        }
    }

    private void EnableTransformInputs()
    {
        if (dragDeltaAction.action != null)
        {
            dragDeltaAction.action.performed += onDragDelta;
        }

        if (pinchDeltaAction.action != null)
        {
            pinchDeltaAction.action.performed += onPinchDelta;
        }
    }

    private void EnableControllers()
    {
        if (ScalingModeController.Instance != null)
        {
            ScalingModeController.Instance.modeChanged += OnModeChange;    
        }

        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform += ResetRotation;    
        }
        ObjectSpawnerAR.objectSpawnedEvent += HandleSpawnPosition;
    }

    private void DisableTransformInputs()
    {
        if (dragDeltaAction.action != null)
        {
            dragDeltaAction.action.performed -= onDragDelta;
        }

        if (pinchDeltaAction.action != null)
        {
            pinchDeltaAction.action.performed -= onPinchDelta;
        }
    }

    private void DisableControllers()
    {
        if (ScalingModeController.Instance != null)
        {
            ScalingModeController.Instance.modeChanged -= OnModeChange;    
        }

        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform -= ResetRotation;    
        }
        ObjectSpawnerAR.objectSpawnedEvent -= HandleSpawnPosition;
    }

    private void OnEnable()
    {
        EnableTransformInputs();
        EnableControllers();
    }

    private void OnDisable()
    {
        DisableTransformInputs();
        DisableControllers();
    }

    public void OnModeChange(bool isOneToOne)
    {
        if (isOneToOne) OneToOneScale();
        else FitToScreenScale();
    }

    void onDragDelta(InputAction.CallbackContext ctx)
    {
        if (patentManager.ActivePatent == null) return;

        Vector2 delta = ctx.ReadValue<Vector2>();
        float yDegrees = -delta.x * dragRotationSpeed;
        float xDegrees = delta.y * dragRotationSpeed;

        Quaternion rotY = Quaternion.AngleAxis(yDegrees, Vector3.up);
        Quaternion rotX = Quaternion.AngleAxis(xDegrees, Camera.main.transform.right);
        
        targetRotation = rotX * rotY * targetRotation;
    }

    void onPinchDelta(InputAction.CallbackContext ctx)
    {
        if (patentManager.ActivePatent == null) return;

        float pinchDelta = ctx.ReadValue<float>();
        float scaleFactor = 1f + pinchDelta * pinchScaleSpeed;
        
        Vector3 newScale = targetScale * scaleFactor;
        float clamped = Mathf.Clamp(newScale.x, minScale, maxScale);
        targetScale = new Vector3(clamped, clamped, clamped);
    }

    public void HandleSpawnPosition(bool toGround)
    {
        GameObject activePatent = patentManager.ActivePatent;
        if (activePatent == null) return;

        if (visualPivot != null) Destroy(visualPivot);

        float scale = ScreenScaler.instance.FitScreen(activePatent);
        if (scale > 0) activePatent.transform.localScale *= scale;
        
        PivotSetter.SnapToYOffset(activePatent, toGround);

        Bounds localBounds = PivotSetter.GetBounds(activePatent);
        Vector3 worldCenter = activePatent.transform.TransformPoint(localBounds.center);

        visualPivot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualPivot.name = "PivotIndicator";
        visualPivot.transform.position = worldCenter;
        visualPivot.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        visualPivot.transform.SetParent(activePatent.transform, true);

        SyncTargets();
    }

    public void ResetRotation()
    {
        if (patentManager.ActivePatent == null) return;
        targetRotation = patentManager.initialRotation;
        targetScale = patentManager.initialScale;
        patentManager.ActivePatent.transform.position = patentManager.initialPosition;
    }

    private void SyncTargets()
    {
        if (patentManager.ActivePatent == null) return;
        targetRotation = patentManager.ActivePatent.transform.rotation;
        targetScale = patentManager.ActivePatent.transform.localScale;
    }

    public void FitToScreenScale()
    {
        if (patentManager.Patent == null || patentManager.ActivePatent == null) return;
        ResetRotation();
        EnableTransformInputs();
    }

    public void OneToOneScale()
    {
        if (patentManager.Patent == null || patentManager.ActivePatent == null) return;
        targetScale = Vector3.one;
        targetRotation = patentManager.initialRotation;
        PivotSetter.ChangeToOneOneMode(patentManager.ActivePatent, Camera.main.transform, 1);
        DisableTransformInputs();
    }
}