using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class PopupMenuManager : MonoBehaviour
{
    public static PopupMenuManager Instance;

    [SerializeField] private UIToggle boundingBoxPopup;

    [SerializeField] private UIToggle lightPopup;

    [SerializeField] private UIToggle descriptionPopup;
    
    [SerializeField] private UIToggle lodToggle;

    [SerializeField] private List<TextMeshProUGUI> descriptions;
    
    public event UnityAction<bool> OnEnableBoundingBox;
    public event UnityAction<bool> OnEnableLightning;
    public event UnityAction<bool> OnEnableDescriptions;
    public event UnityAction<bool> OnLodChange; 

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

    private void OnEnable()
    {
        boundingBoxPopup.OnToggleValueChanged += EnableBoundingBox;
        lightPopup.OnToggleValueChanged += EnableLightning;
        descriptionPopup.OnToggleValueChanged += EnableDescriptions;
        lodToggle.OnToggleValueChanged += OnLodChangeToggle;
    }

    private void OnDisable()
    {
        boundingBoxPopup.OnToggleValueChanged -= EnableBoundingBox;
        lightPopup.OnToggleValueChanged -= EnableLightning;
        descriptionPopup.OnToggleValueChanged -= EnableDescriptions;
        lodToggle.OnToggleValueChanged -= OnLodChangeToggle;
    }

    void OnLodChangeToggle(bool isOn)
    {
        Debug.Log("lod level is " + isOn);
        OnLodChange?.Invoke(isOn);
    }

    void EnableBoundingBox(bool isOn)
    {
        Debug.Log("bounding box is: "  + isOn);
        OnEnableBoundingBox?.Invoke(isOn);
    }

    void EnableLightning(bool isOn)
    {
        Debug.Log("lightning is: "  + isOn);
        OnEnableLightning?.Invoke(isOn);
    }

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
