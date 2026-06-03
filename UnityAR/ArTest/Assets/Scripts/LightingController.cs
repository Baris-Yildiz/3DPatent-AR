using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LightingController : MonoBehaviour
{
    [SerializeField] private PopupMenuManager popupMenuManager;
    [SerializeField] private ARCameraManager arCameraManager;

    private LightEstimation _savedLightEstimation;
    private CameraFacingDirection _savedFacingDirection;
    private AmbientMode _savedAmbientMode;
    private Color _savedAmbientLight;

    private void Start()
    {
        if (popupMenuManager != null)
            popupMenuManager.OnEnableLightning += SetLighting;
        else
            Debug.LogError("Pop up manager is null");

        if (arCameraManager != null)
        {
            _savedLightEstimation = arCameraManager.requestedLightEstimation;
            _savedFacingDirection = arCameraManager.requestedFacingDirection;
        }

        _savedAmbientMode = RenderSettings.ambientMode;
        _savedAmbientLight = RenderSettings.ambientLight;

        SetLighting(false);
    }

    private void OnDestroy()
    {
        if (popupMenuManager != null)
            popupMenuManager.OnEnableLightning -= SetLighting;
    }

    private void SetLighting(bool enable)
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("Current render pipeline is not URP.");
            return;
        }

        var field = typeof(UniversalRenderPipelineAsset).GetField("m_MainLightRenderingMode",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogError("Could not find m_MainLightRenderingMode field via reflection.");
            return;
        }

        field.SetValue(urpAsset, enable ? LightRenderingMode.PerPixel : LightRenderingMode.Disabled);

        RenderSettings.ambientMode = enable ? _savedAmbientMode : AmbientMode.Flat;
        RenderSettings.ambientLight = enable ? _savedAmbientLight : Color.white;

        if (arCameraManager != null)
        {
            arCameraManager.requestedLightEstimation = enable ? _savedLightEstimation : LightEstimation.None;
            arCameraManager.requestedFacingDirection = _savedFacingDirection;
        }
    }
}
