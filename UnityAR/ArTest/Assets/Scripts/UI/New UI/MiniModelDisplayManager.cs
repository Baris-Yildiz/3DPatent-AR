using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class MiniModelDisplayManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Image m_MiniDisplayImage;
    private int m_DisplayWidth;
    private int m_DisplayHeight;

    public LayerMask ModelImageLayer;
    private Sprite m_DefaultSprite;

    void Start()
    {
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateExit += () => DisplayMiniObject();

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => ResetDisplay();

        m_MiniDisplayImage = GetComponent<Image>();
        m_DefaultSprite = m_MiniDisplayImage.sprite;

        RectTransform rt = GetComponent<RectTransform>();
        m_DisplayHeight = (int)rt.rect.height;
        m_DisplayWidth = (int)rt.rect.width;
    }

    private void ResetDisplay()
    {
        m_MiniDisplayImage.sprite = m_DefaultSprite;
    }


    private void DisplayMiniObject()
    {
        //TODO: add camera to models that dont have one
        //TODO: better adjust size of camera to perfectly fit model, now it is up to the model's own config.
        int width = Screen.width;
        int height = Screen.height;

        GameObject loadedModel = QRNetworkHandler.Instance.LoadedModel;
        loadedModel.SetLayerRecursively(ModelImageLayer);
        
        Camera renderCamera = loadedModel.GetComponentInChildren<Camera>(true);
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        renderCamera.cullingMask = 1 << ModelImageLayer;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = 1.5f;

        RenderTexture cameraRender = new RenderTexture(width, height, 24);
        renderCamera.targetTexture = cameraRender;
        renderCamera.Render();

        RenderTexture.active = cameraRender;
        Texture2D displayTexture = new Texture2D(width,height, TextureFormat.ARGB32, false);
        displayTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        displayTexture.Apply();

        Sprite sprite = Sprite.Create(displayTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        m_MiniDisplayImage.sprite = sprite;
        
    }
}
