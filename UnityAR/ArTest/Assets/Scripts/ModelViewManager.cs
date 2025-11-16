using UnityEngine;
using UnityEngine.UI;

public class ModelViewManager : MonoBehaviour
{
    public Button NormalViewButton;
    public Button OneToOneViewButton;

    public static ModelViewManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NormalViewButton.onClick.AddListener(SetNormalView);
        OneToOneViewButton.onClick.AddListener(SetOneToOneView);

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetNormalView()
    {

    }

    private void SetOneToOneView()
    {

    }
}
