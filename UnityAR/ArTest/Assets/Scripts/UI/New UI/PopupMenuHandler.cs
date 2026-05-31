using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PopupMenuHandler : MonoBehaviour
{

    [SerializeField] private GameObject popupMenu;

    private Button button;

    private bool isOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(EnablePopupMenu);
    }
    
    void EnablePopupMenu()
    {
        isOpen = !isOpen;
        popupMenu.SetActive(isOpen);
    }
}
