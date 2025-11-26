using UnityEngine;


enum ScalingMode
{
    FitScreen,
    OneToOne,
}

public class ScalingModeController : MonoBehaviour
{

    [SerializeField] private ScalingMode mode = ScalingMode.FitScreen;
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMode(ScalingMode mode)
    {
        
    }
}
