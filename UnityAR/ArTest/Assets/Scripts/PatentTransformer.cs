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
    [SerializeField] private bool usePivot = true;

    private Quaternion targetRotation;
    private Vector3 targetScale;
    
    private Vector3 GetRotationCenter(GameObject obj)
    {
        if (usePivot)
        {
            return obj.transform.position;
        }
        else
        {
            Bounds localBounds = PivotSetter.GetBounds(obj);
            return obj.transform.TransformPoint(localBounds.center);
        }
    }


    private void Start()
    {
        patentManager = PatentManager.Instance;

    }

    private void Update()
    {
        if (patentManager.ActivePatent == null) return;

        Transform t = patentManager.ActivePatent.transform;


        t.localScale = Vector3.Lerp(t.localScale, targetScale, Time.deltaTime * smoothSpeed);

        if (Quaternion.Angle(t.rotation, targetRotation) > 0.01f)
        {

            Vector3 worldCenter = GetRotationCenter(patentManager.ActivePatent);
            
            Vector3 offset = t.position - worldCenter;
            
            Quaternion nextRotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * smoothSpeed);

   
            Quaternion delta = nextRotation * Quaternion.Inverse(t.rotation);
            Vector3 rotatedOffset = delta * offset;

            t.rotation = nextRotation;
            t.position = worldCenter + rotatedOffset;
        }
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


    private void EnableTransformInputs()
    {
        if (dragDeltaAction.action != null)
        {
            dragDeltaAction.action.performed += OnDragDelta;
            
        }


        if (pinchDeltaAction.action != null)
        {
            pinchDeltaAction.action.performed += OnPinchDelta;    
        }

        
    }

    private void DisableTransformInputs()
    {
        if (dragDeltaAction.action != null)
        {
            dragDeltaAction.action.performed -= OnDragDelta;
           
        }

        

        if (pinchDeltaAction.action != null)
            pinchDeltaAction.action.performed -= OnPinchDelta;
    }

    private void UpdateMouseRotation()
    {
        if (!useMouseFallback) return;
        if (patentManager.ActivePatent == null) return;
        if (!Input.GetMouseButton(0)) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (mouseX == 0f && mouseY == 0f) return;

        float yDegrees = -mouseX * mouseDragToRotate;
        float xDegrees =  mouseY * mouseDragToRotate;

        Quaternion rotY = Quaternion.AngleAxis(yDegrees, Vector3.up);
        Quaternion rotX = Quaternion.AngleAxis(xDegrees, Camera.main.transform.right);

        targetRotation = rotX * rotY * targetRotation;
    }


    private void EnableControllers()
    {
        if (ScalingModeController.Instance != null)
            ScalingModeController.Instance.modeChanged += OnModeChange;

        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform += ResetRotation;
            ModelTransformManager.Instance.OnLockModel += ChangeTransformInputState;
        }

        ObjectSpawnerAR.objectSpawnedEvent += HandleSpawnPosition;
    }

    private void DisableControllers()
    {
        if (ScalingModeController.Instance != null)
            ScalingModeController.Instance.modeChanged -= OnModeChange;

        if (ModelTransformManager.Instance != null)
        {
            ModelTransformManager.Instance.OnResetModelTransform -= ResetRotation;
            ModelTransformManager.Instance.OnLockModel -= ChangeTransformInputState;
        }

        ObjectSpawnerAR.objectSpawnedEvent -= HandleSpawnPosition;
    }

    private void ChangeTransformInputState(bool isLocked)
    {
        if (isLocked) DisableTransformInputs();
        else          EnableTransformInputs();
    }


    private void OnDragDelta(InputAction.CallbackContext ctx)
    {
        if (patentManager.ActivePatent == null) return;

        Vector2 delta = ctx.ReadValue<Vector2>();
        float yDegrees = -delta.x * dragRotationSpeed;
        float xDegrees =  delta.y * dragRotationSpeed;

        Quaternion rotY = Quaternion.AngleAxis(yDegrees, Vector3.up);
        Quaternion rotX = Quaternion.AngleAxis(xDegrees, Camera.main.transform.right);

        targetRotation = rotX * rotY * targetRotation;
    }

    private void OnPinchDelta(InputAction.CallbackContext ctx)
    {
        if (patentManager.ActivePatent == null) return;

        float pinchDelta  = ctx.ReadValue<float>();
        float scaleFactor = 1f + pinchDelta * pinchScaleSpeed;

        Vector3 newScale = targetScale * scaleFactor;
        float   clamped  = Mathf.Clamp(newScale.x, minScale, maxScale);
        targetScale = new Vector3(clamped, clamped, clamped);
    }


    public void OnModeChange(bool isOneToOne)
    {
        if (isOneToOne) OneToOneScale();
        else            FitToScreenScale();
    }

    public void HandleSpawnPosition(bool toGround)
    {
        GameObject activePatent = patentManager.ActivePatent;
        if (activePatent == null) return;

        float scale = ScreenScaler.instance.FitScreen(activePatent, usePivot);
        if (scale > 0) activePatent.transform.localScale *= scale;

        PivotSetter.SnapToYOffset(activePatent, toGround, usePivot);
        SyncTargets();
    }

    public void ResetRotation()
    {
        if (patentManager.ActivePatent == null) return;
        targetRotation = patentManager.initialRotation;
        targetScale    = patentManager.initialScale;
        patentManager.ActivePatent.transform.position = patentManager.initialPosition;
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
        targetScale    = Vector3.one;
        targetRotation = patentManager.initialRotation;
        PatentManager.Instance.ActivePatent.transform.rotation = PatentManager.Instance.initialRotation;
        PivotSetter.ChangeToOneOneMode(patentManager.ActivePatent, Camera.main.transform, 1);
        DisableTransformInputs();
    }


    private void SyncTargets()
    {
        if (patentManager.ActivePatent == null) return;
        targetRotation = patentManager.ActivePatent.transform.rotation;
        targetScale    = patentManager.ActivePatent.transform.localScale;
    }
}
