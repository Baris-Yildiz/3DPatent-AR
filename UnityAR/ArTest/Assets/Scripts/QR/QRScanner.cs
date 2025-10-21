using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditorInternal;
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
    StateMachine m_StateMachine;

    private void Start()
    {
        m_BarcodeReader = new BarcodeReader();

        width = Screen.width;
        height = Screen.height;
        QrCode = string.Empty;

        m_StateMachine = GameObject.FindWithTag("StateMachine").GetComponent<StateMachine>();
    }

    public void ResetScanner()
    {
        QrCode = string.Empty;
    }

    public void ScanScreen()
    {
        StartCoroutine(ScanFrameForQRCode(60));
    }

    IEnumerator ScanFrameForQRCode(int waitframeCount)
    {
        while (string.IsNullOrEmpty(QrCode) && m_StateMachine.CurrentStateName == StateMachine.States.QR_SCAN_STATE)
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

                var Result = m_BarcodeReader.Decode(currentFrame.GetRawTextureData(), width, height, RGBLuminanceSource.BitmapFormat.ARGB32);
                QrCode = Result?.Text;
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }
        }
        if (!string.IsNullOrEmpty(QrCode))
        {
            m_StateMachine.SetState(StateMachine.States.QR_SCAN_COMPLETE_STATE);
        }
        
    }
}
