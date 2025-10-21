using System.Collections;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class QRNetworkHandler : MonoBehaviour
{
    QRScanner m_QRScanner;

    private void Start()
    {
        m_QRScanner = GameObject.FindWithTag("QRScanner").GetComponent<QRScanner>();
    }

    public void StartDownloadingModel()
    {
        StartCoroutine(DownloadModel(m_QRScanner.QrCode));
    }

    private void OnDownloadProgress(UnityWebRequest www)
    {
        print(www.downloadProgress.ToString("0.000000"));
    }

    private IEnumerator DownloadModel(string uri)
    {
        
        using(UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            www.SendWebRequest();
            
            while (www.downloadProgress < 1)
            {
                OnDownloadProgress(www);
                yield return new WaitForEndOfFrame();
            }

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError ||
                www.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Error: " + www.error);
            } else
            {
                string path = Path.Combine(Application.persistentDataPath, "test.glb");
                File.WriteAllBytes(path, www.downloadHandler.data);
                Debug.Log("File downloaded at path: " + path);
            }
        }
    }
}
