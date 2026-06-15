using System;
using System.Collections;

using TMPro;

using UnityEngine;

using ZXing;
using ZXing.QrCode;

/// <summary>
/// Singleton component that reads screen pixels every N frames and decodes them
/// as a QR code using the ZXing library. When a code is found the state machine
/// transitions to <see cref="StateMachine.States.QR_SCAN_COMPLETE_STATE"/> and
/// the decoded URL is available via <see cref="QrCode"/>.
/// </summary>
public class QRScanner : MonoBehaviour
{
    Texture2D currentFrame;

    int width;
    int height;

    /// <summary>The decoded QR code string, or <see cref="string.Empty"/> when no code has been found.</summary>
    public string QrCode { get; private set; }

    /// <summary>The single active instance of <see cref="QRScanner"/>.</summary>
    public static QRScanner Instance
    {
        get; private set;
    }

    IBarcodeReader m_BarcodeReader;

    /// <summary>Initialises the singleton instance.</summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Creates the ZXing barcode reader, subscribes to the QR scan state,
    /// and caches the screen resolution used for pixel capture.
    /// </summary>
    private void Start()
    {
        m_BarcodeReader = new BarcodeReader();

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => StartScanner();

        width = Screen.width;
        height = Screen.height;
        QrCode = string.Empty;
    }

    /// <summary>
    /// Resets the scanner state and starts a new scan coroutine.
    /// Safe to call while a scan is already running.
    /// </summary>
    public void StartScanner()
    {
        ResetScanner();
        ScanScreen();
    }

    /// <summary>
    /// Stops any active scan coroutine and clears the last decoded result.
    /// </summary>
    public void StopScanner()
    {
        ResetScanner();
        StopAllCoroutines();
    }

    /// <summary>Clears the last decoded QR code string.</summary>
    private void ResetScanner()
    {
        QrCode = string.Empty;
    }

    /// <summary>Starts the <see cref="ScanFrameForQRCode"/> coroutine with a 60-frame interval.</summary>
    private void ScanScreen()
    {
        StartCoroutine(ScanFrameForQRCode(60));
    }

    /// <summary>
    /// Coroutine that captures a screen frame every <paramref name="waitframeCount"/>
    /// frames and attempts to decode it as a QR code. Stops automatically once a
    /// code is found or the state changes away from
    /// <see cref="StateMachine.States.QR_SCAN_STATE"/>.
    /// </summary>
    /// <param name="waitframeCount">Number of frames to skip between each decode attempt.</param>
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
