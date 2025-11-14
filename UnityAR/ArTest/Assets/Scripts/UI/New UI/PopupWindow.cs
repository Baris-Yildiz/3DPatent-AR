using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;



public class PopupWindow : MonoBehaviour
{
    [SerializeField] private GameObject m_Content;
    [SerializeField] private GameObject m_Header;
    [SerializeField] private GameObject m_Footer;

    public static Color BUTTON_COLOR_CANCEL = new (0.7529412f, 0.2235294f, 0.1686275f);
    public static Color BUTTON_COLOR_OK = new (0.4980392f, 0.5490196f, 0.5529412f);

    private static int m_LastWindowID = -1;
    public int WindowID { get; private set; }

    private void Start()
    {
        m_LastWindowID++;
        WindowID = m_LastWindowID;
    }

    public void AddContentText(string content)
    {
        GameObject textObject = new GameObject();
        textObject.transform.SetParent(m_Content.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(500f, rectTransform.sizeDelta.y);

        ContentSizeFitter contentSizeFitter = textObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    public void AddHeader(string headerText)
    {
        GameObject textObject = new GameObject();
        textObject.transform.SetParent(m_Header.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = headerText;
        text.fontStyle = FontStyles.Bold;

        ContentSizeFitter contentSizeFitter = textObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    public void AddContent(GameObject content)
    {
        if (!content.activeSelf) content.SetActive(true);
        content.transform.SetParent(m_Content.transform, false);
    }

    public void AddFooterButton(string buttonText, Color buttonColor, UnityAction callback)
    {
        GameObject button = new GameObject();
        button.transform.SetParent(m_Footer.transform, false);

        UnityEngine.UI.Image imageComponent = button.AddComponent<UnityEngine.UI.Image>();
        imageComponent.color = buttonColor;
        imageComponent.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(150f, 50f);

        Button buttonComponent = button.AddComponent<Button>();

        buttonComponent.onClick.AddListener(callback);

        GameObject child = new GameObject();
        child.transform.SetParent(button.transform, false);

        TextMeshProUGUI textComponent = child.AddComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.Midline;
        textComponent.enableAutoSizing = true;

        rectTransform = textComponent.rectTransform;

        rectTransform.anchorMin = Vector2.zero; 
        rectTransform.anchorMax = Vector2.one;  
        
        rectTransform.offsetMin = Vector2.zero; 
        rectTransform.offsetMax = Vector2.zero;

        textComponent.text = buttonText;
    }

}
