using UnityEngine;

public class MiscOptionsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponentInChildren<UIVisibilityManager>().Initialize();
        GetComponentInChildren<ModelPlaneDetectionManager>().Initialize();
        gameObject.SetActive(false);
    }
}
