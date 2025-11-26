using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour
{
    private Toggle m_Toggle;
    public event UnityAction<bool> OnToggleValueChanged;
    private Image m_Image;
    private Color m_DefaultColor;

    void Awake()
    {
        m_Image = GetComponent<Image>();
        m_DefaultColor = m_Image.color;

        m_Toggle = GetComponent<Toggle>();

        m_Toggle.onValueChanged.AddListener(OnTogglePress);
    }

    private void OnTogglePress(bool isOn)
    {
        ChangeColor(isOn);
        OnToggleValueChanged?.Invoke(isOn);
    }

    private void ChangeColor(bool isOn)
    {
        if (isOn)
        {
            m_Image.color = Color.green;
        }
        else
        {
            m_Image.color = m_DefaultColor;
        }
    }
}
