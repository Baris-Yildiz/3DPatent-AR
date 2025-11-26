using System;
using UnityEngine;


enum ScalingMode
{
    FitScreen,
    OneToOne,
}

public class ScalingModeController : MonoBehaviour
{

    [SerializeField] private ScalingMode mode = ScalingMode.FitScreen;
    public static event Action<bool> modeChanged;
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
