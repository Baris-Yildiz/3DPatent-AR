using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRScanner : MonoBehaviour
{
    Texture2D currentFrame;

    int width;
    int height;

    public string QrCode { get;  private set; }

    IBarcodeReader m_BarcodeReader;

    private void Start()
    {
        m_BarcodeReader = new BarcodeReader();

        width = Screen.width;
        height = Screen.height;
        QrCode = string.Empty;
    }

    public void ResetScanner()
    {
        QrCode = string.Empty;
    }

    public void ScanScreen()
    {
        StartCoroutine(ScanFrameForQRCode());
    }

    IEnumerator ScanFrameForQRCode()
    {
        currentFrame = new Texture2D(width, height, TextureFormat.ARGB32, false);
        yield return new WaitForEndOfFrame();

        try
        {
            currentFrame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            currentFrame.Apply();

            var Result = m_BarcodeReader.Decode(currentFrame.GetRawTextureData(), width, height, RGBLuminanceSource.BitmapFormat.ARGB32);
            QrCode = Result?.Text;
        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }

        yield return null;
    }
}
