using System;
using UnityEngine;
using UnityEngine.UI;

public class ModelPlaneDetectionManager : MonoBehaviour
{
    GameObject m_Parent;

    public static ModelPlaneDetectionManager Instance { get; private set; }
    public event Action<bool> OnPlaneDetectionChange;

    private void Awake()
    {
        Instance = this;
    }

    private Toggle m_ToggleComponent;

    void Start()
    {
        m_Parent = gameObject.transform.parent.gameObject;
        m_Parent.SetActive(false);

        m_ToggleComponent = GetComponent<Toggle>();
    }

    public void OnPlaneDetectionToggleChange(bool isOn)
    {
        OnPlaneDetectionChange?.Invoke(isOn);
        //planeDetectionToggleChange?.Invoke(isOn);
    }
}
