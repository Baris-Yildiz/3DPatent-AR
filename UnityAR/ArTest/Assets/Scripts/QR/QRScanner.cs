using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using ZXing;

public class QRScanner : MonoBehaviour
{
    Texture2D currentFrame;
    RawImage debugRenderer;
    public TextMeshProUGUI outputText;
    int width = Screen.width;
    int height = Screen.height;

    string QrCode = string.Empty;

    void Start()
    {
        debugRenderer = GetComponent<RawImage>(); //for debugging: show camera view
        StartCoroutine(GetQRCode());
    }
    
    IEnumerator GetQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();

        while (string.IsNullOrEmpty(QrCode))
        {
            currentFrame = new Texture2D(width, height, TextureFormat.ARGB32, false); //snap is a single capture of camera video frame
            yield return new WaitForEndOfFrame();
            try
            {
                
                currentFrame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                currentFrame.Apply();
                debugRenderer.texture = currentFrame;

                var Result = barCodeReader.Decode(currentFrame.GetRawTextureData(), width, height, RGBLuminanceSource.BitmapFormat.ARGB32);
                if (Result != null)
                {
                    QrCode = Result.Text;
                    if (!string.IsNullOrEmpty(QrCode))
                    {
                        outputText.text = "DECODED TEXT FROM QR: " + QrCode;
                        break;
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }

            yield return null;
        }

    }
}
