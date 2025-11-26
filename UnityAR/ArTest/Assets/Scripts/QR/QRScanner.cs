using System;
using System.Collections;

using TMPro;

using UnityEngine;

using ZXing;
using ZXing.QrCode;

public class QRScanner : MonoBehaviour
{
    Texture2D currentFrame;

    int width;
    int height;

    public string QrCode { get;  private set; }
    public static QRScanner Instance
    {
        get; private set;
    }

    IBarcodeReader m_BarcodeReader;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_BarcodeReader = new BarcodeReader();

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => StartScanner();

        width = Screen.width;
        height = Screen.height;
        QrCode = string.Empty;
    }

    public void StartScanner()
    {
        ResetScanner();
        ScanScreen();
    }

    public void StopScanner()
    {
        ResetScanner();
        StopAllCoroutines();
    }

    private void ResetScanner()
    {
        QrCode = string.Empty;
    }
    private void ScanScreen()
    {
        StartCoroutine(ScanFrameForQRCode(60));
    }

    IEnumerator ScanFrameForQRCode(int waitframeCount)
    {
        while (string.IsNullOrEmpty(QrCode) && StateMachine.Instance.CurrentStateName == StateMachine.States.QR_SCAN_STATE)
        {
            currentFrame = new Texture2D(width, height, TextureFormat.ARGB32, false);

            for (int i = 0; i < waitframeCount; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            try
            {
                currentFrame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                currentFrame.Apply();
                var pixelData = currentFrame.GetRawTextureData();
                var Result = m_BarcodeReader.Decode(pixelData, width, height, RGBLuminanceSource.BitmapFormat.ARGB32);
                QrCode = Result?.Text;
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }
        }
        if (!string.IsNullOrEmpty(QrCode))
        {
            StateMachine.Instance.SetState(StateMachine.States.QR_SCAN_COMPLETE_STATE);
        }
        
    }
}
