using System;
using UnityEngine;

public class DontDestroyObjects : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
