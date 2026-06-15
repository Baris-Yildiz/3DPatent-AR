using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Singleton that owns the in-AR options popup menu, forwarding toggle states for
/// bounding-box visibility, lighting, model descriptions, and LOD level to
/// interested subscribers via Unity events.
/// </summary>
public class PopupMenuManager : MonoBehaviour
{
    /// <summary>The single active instance of <see cref="PopupMenuManager"/>.</summary>
    public static PopupMenuManager Instance;

    [SerializeField] private UIToggle boundingBoxPopup;
    [SerializeField] private UIToggle lightPopup;
    [SerializeField] private UIToggle descriptionPopup;
    [SerializeField] private UIToggle lodToggle;
    [SerializeField] private List<TextMeshProUGUI> descriptions;

    /// <summary>Fired when the bounding-box toggle changes. Parameter is the new state.</summary>
    public event UnityAction<bool> OnEnableBoundingBox;

    /// <summary>Fired when the lighting toggle changes. Parameter is the new state.</summary>
    public event UnityAction<bool> OnEnableLightning;

    /// <summary>Fired when the description toggle changes. Parameter is the new state.</summary>
    public event UnityAction<bool> OnEnableDescriptions;

    /// <summary>Fired when the LOD toggle changes. Parameter is <c>true</c> for LOD 1 (simplified).</summary>
    public event UnityAction<bool> OnLodChange;

    /// <summary>Initialises the singleton instance.</summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Disables raycasting on all description text objects so they do not
    /// block touch input on the model beneath them.
    /// </summary>
    private void Start()
    {
        if (descriptions == null) return;
        foreach (TextMeshProUGUI text in descriptions)
        {
            if (text != null)
            {
                text.raycastTarget = false;
            }
        }
    }

    /// <summary>Subscribes all toggle callbacks when the menu becomes active.</summary>
    private void OnEnable()
    {
        boundingBoxPopup.OnToggleValueChanged += EnableBoundingBox;
        lightPopup.OnToggleValueChanged += EnableLightning;
        descriptionPopup.OnToggleValueChanged += EnableDescriptions;
        lodToggle.OnToggleValueChanged += OnLodChangeToggle;
    }

    /// <summary>Unsubscribes all toggle callbacks when the menu is deactivated.</summary>
    private void OnDisable()
    {
        boundingBoxPopup.OnToggleValueChanged -= EnableBoundingBox;
        lightPopup.OnToggleValueChanged -= EnableLightning;
        descriptionPopup.OnToggleValueChanged -= EnableDescriptions;
        lodToggle.OnToggleValueChanged -= OnLodChangeToggle;
    }

    /// <summary>Forwards the LOD toggle state to <see cref="OnLodChange"/> subscribers.</summary>
    /// <param name="isOn"><c>true</c> to switch to the simplified LOD.</param>
    void OnLodChangeToggle(bool isOn)
    {
        Debug.Log("lod level is " + isOn);
        OnLodChange?.Invoke(isOn);
    }

    /// <summary>Forwards the bounding-box toggle state to <see cref="OnEnableBoundingBox"/> subscribers.</summary>
    /// <param name="isOn"><c>true</c> to show the bounding box.</param>
    void EnableBoundingBox(bool isOn)
    {
        Debug.Log("bounding box is: "  + isOn);
        OnEnableBoundingBox?.Invoke(isOn);
    }

    /// <summary>Forwards the lighting toggle state to <see cref="OnEnableLightning"/> subscribers.</summary>
    /// <param name="isOn"><c>true</c> to enable AR lighting estimation.</param>
    void EnableLightning(bool isOn)
    {
        Debug.Log("lightning is: "  + isOn);
        OnEnableLightning?.Invoke(isOn);
    }

    /// <summary>
    /// Shows or hides all description text objects and forwards the new state
    /// to <see cref="OnEnableDescriptions"/> subscribers.
    /// </summary>
    /// <param name="isOn"><c>true</c> to show descriptions.</param>
    void EnableDescriptions(bool isOn)
    {
        if (descriptions == null) return;
        Debug.Log("Descriptions are : "  + isOn);
        foreach (TextMeshProUGUI text in descriptions)
        {
            if (text != null)
            {
                text.gameObject.SetActive(isOn);
            }


        }
        OnEnableDescriptions?.Invoke(isOn);
    }
}
