using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightingController : MonoBehaviour
{
    [SerializeField] private PopupMenuManager popupMenuManager;
    private AmbientMode _savedAmbientMode;
    private Color _savedAmbientLight;
    private float _savedAmbientIntensity;
    private Material _savedSkybox;
    private float _savedReflectionIntensity;
    private ShadowQuality _savedShadowQuality;

    private struct LightState
    {
        public Light light;
        public bool enabled;
        public LightShadows shadows;
    }

    private struct RendererState
    {
        public Renderer renderer;
        public ShadowCastingMode castingMode;
        public bool receiveShadows;
    }

    private readonly List<LightState> _savedLightStates = new();
    private readonly List<RendererState> _savedRendererStates = new();

    private void Start()
    {
        if (popupMenuManager != null)
        {
            popupMenuManager.OnEnableLightning += SetLighting;    
        }
        else
        {
            Debug.LogError("Pop up manager is null");
        }


    }

    private void OnDestroy()
    {
        if (popupMenuManager != null)
        {
            popupMenuManager.OnEnableLightning -= SetLighting;
        }
        else
        {
            Debug.LogError("Pop up manager is null");
        }

        
    }

    private void SetLighting(bool enable)
    {
        Debug.Log("Setting Lightning : " + enable);
        if (enable)
            RestoreLighting();
        else
            DisableLighting();
    }

    private void DisableLighting()
    {
        _savedAmbientMode = RenderSettings.ambientMode;
        _savedAmbientLight = RenderSettings.ambientLight;
        _savedAmbientIntensity = RenderSettings.ambientIntensity;
        _savedSkybox = RenderSettings.skybox;
        _savedReflectionIntensity = RenderSettings.reflectionIntensity;
        _savedShadowQuality = QualitySettings.shadows;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.skybox = null;
        RenderSettings.reflectionIntensity = 0f;
        QualitySettings.shadows = ShadowQuality.Disable;

        _savedLightStates.Clear();
        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            _savedLightStates.Add(new LightState { light = l, enabled = l.enabled, shadows = l.shadows });
            l.shadows = LightShadows.None;
            l.enabled = false;
        }

        _savedRendererStates.Clear();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            _savedRendererStates.Add(new RendererState
            {
                renderer = r,
                castingMode = r.shadowCastingMode,
                receiveShadows = r.receiveShadows
            });
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
    }

    private void RestoreLighting()
    {
        RenderSettings.ambientMode = _savedAmbientMode;
        RenderSettings.ambientLight = _savedAmbientLight;
        RenderSettings.ambientIntensity = _savedAmbientIntensity;
        RenderSettings.skybox = _savedSkybox;
        RenderSettings.reflectionIntensity = _savedReflectionIntensity;
        QualitySettings.shadows = _savedShadowQuality;

        foreach (LightState s in _savedLightStates)
        {
            if (s.light == null) continue;
            s.light.shadows = s.shadows;
            s.light.enabled = s.enabled;
        }

        foreach (RendererState s in _savedRendererStates)
        {
            if (s.renderer == null) continue;
            s.renderer.shadowCastingMode = s.castingMode;
            s.renderer.receiveShadows = s.receiveShadows;
        }
    }
}
