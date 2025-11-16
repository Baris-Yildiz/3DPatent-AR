using UnityEngine;
using UnityEngine.UI;

public class ModelTransformManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Button LockModelButton;
    public Button ResetModelTransformButton;
    public Button DeleteModelTransformButton;
    
    public static ModelTransformManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
