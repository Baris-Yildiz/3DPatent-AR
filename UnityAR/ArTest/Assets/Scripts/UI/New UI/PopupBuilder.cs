using UnityEngine;
using UnityEngine.Events;

public class PopupBuilder
{
    private GameObject m_PopupWindowPrefab;
    private PopupWindow m_PopupWindowScript;

    private const string m_PopupWindowPrefabPath = "Popup Window";

    public static PopupBuilder Create()
    {
        return new PopupBuilder();
    }

    private PopupBuilder()
    {
        
        m_PopupWindowPrefab = Resources.Load<GameObject>(m_PopupWindowPrefabPath);
        m_PopupWindowPrefab = GameObject.Instantiate(m_PopupWindowPrefab, GameObject.FindWithTag("PopupParent").transform);
        
        m_PopupWindowScript = m_PopupWindowPrefab.GetComponent<PopupWindow>();
        PopupWindow.PopupLayer = GameObject.FindWithTag("PopupLayer");
    }

    public PopupBuilder WithContentText(string text)
    {
        m_PopupWindowScript.AddContentText(text);
        return this;
    }

    public PopupBuilder WithHeader(string text)
    {
        m_PopupWindowScript.AddHeader(text);
        return this;
    }

    public PopupBuilder WithFooterButton(string buttonText, Color color, UnityAction callback)
    {
        m_PopupWindowScript.AddFooterButton(buttonText, color, callback);
        return this;
    }

    public PopupBuilder WithContent(GameObject content)
    {
        m_PopupWindowScript.AddContent(content);
        return this;
    }

    public GameObject Get()
    {
        return m_PopupWindowPrefab;
    }
   
}
