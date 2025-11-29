using UnityEngine;

public class PopupLayerManager : MonoBehaviour
{

    void Awake()
    {
        PopupWindow.PopupLayer = gameObject;
        gameObject.SetActive(false);
    }
}
