using System;
using UnityEngine;
using UnityEngine.UI;

public class PlaneDetectionController : MonoBehaviour
{
    
    private Text toggleText;
    private Toggle toggle;
    public event Action<bool> planeDetectionToggleChange; 

    private void Awake()
    {
        toggleText = GetComponentInChildren<Text>();
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnPlaneDetectionToggleChange);
        toggleText.color = Color.green;
    }

    public void OnPlaneDetectionToggleChange(bool isOn)
    {
        bool t = toggle.isOn;
        Debug.Log("hello from toggle : " + t.ToString());
        toggleText.color = t ? Color.green : Color.red;
        planeDetectionToggleChange?.Invoke(t);
    }
    
}
