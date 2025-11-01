using System;
using UnityEngine;
using UnityEngine.UI;
public class ResetPatentButton : MonoBehaviour
{

    private Button btn;

    public event Action resetPatentTransformAction;
    private void Awake()
    {
        btn = GetComponent<Button>();
        
    }

    private void OnEnable()
    {
        btn.onClick.AddListener(ResetTransform);
    }

    private void OnDisable()
    {
        btn.onClick.RemoveListener(ResetTransform);
    }

    private void ResetTransform()
    {
        resetPatentTransformAction?.Invoke();
    }
}
