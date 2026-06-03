using UnityEngine;

public class LodManager : MonoBehaviour
{
    [SerializeField] private PopupMenuManager popupMenuManager;

    private void Start()
    {
        if (popupMenuManager != null)
            popupMenuManager.OnLodChange += OnLodChange;
        else
            Debug.LogError("PopupMenuManager is null on LodManager");
    }

    private void OnDestroy()
    {
        if (popupMenuManager != null)
            popupMenuManager.OnLodChange -= OnLodChange;
    }

    private void OnLodChange(bool isOn)
    {
        GameObject patent = PatentManager.Instance?.ActivePatent;
        if (patent == null) return;

        int targetLod = isOn ? 1 : 0;
        foreach (LODGroup lodGroup in patent.GetComponentsInChildren<LODGroup>())
            lodGroup.ForceLOD(targetLod);
    }
}
