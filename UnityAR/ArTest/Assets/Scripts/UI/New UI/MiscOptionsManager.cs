using UnityEngine;

/// <summary>
/// Container component for the miscellaneous options panel. On Start it
/// initialises its child <see cref="UIVisibilityManager"/>,
/// <see cref="ModelPlaneDetectionManager"/>, and <see cref="PivotSettingManager"/>
/// components (which require manual initialisation after singletons are ready),
/// then hides the panel until it is needed.
/// </summary>
public class MiscOptionsManager : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<UIVisibilityManager>().Initialize();
        GetComponentInChildren<ModelPlaneDetectionManager>().Initialize();
        GetComponentInChildren<PivotSettingManager>().Initialize();
        gameObject.SetActive(false);
    }
}
