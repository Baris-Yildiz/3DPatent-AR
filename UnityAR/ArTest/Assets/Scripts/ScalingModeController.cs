using System;
using UnityEngine;


enum ScalingMode
{
    FitScreen,
    OneToOne,
}

public class ScalingModeController : MonoBehaviour
{
    public static ScalingModeController Instance;
    [SerializeField] private ScalingMode mode = ScalingMode.FitScreen;
    public  event Action<bool> modeChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        ModelViewManager.Instance.OnOneToOneViewToggle += ChangeModeToOneToOne;
        ModelViewManager.Instance.OnNormalViewToggle += ChangeModeToNormalView;
    }

    public void ChangeModeToOneToOne()
    {
        ChangeMode(true);
    }

    public void ChangeModeToNormalView()
    {
        ChangeMode(false);
    }

    public void ChangeMode(bool isOneToOne)
    {
        if (isOneToOne && mode != ScalingMode.OneToOne)
        {
            mode = ScalingMode.OneToOne;
            modeChanged?.Invoke(true);
        }
        else if (!isOneToOne && mode != ScalingMode.FitScreen)
        {
            mode = ScalingMode.FitScreen;
            modeChanged?.Invoke(false);
        }
        
    }
}
