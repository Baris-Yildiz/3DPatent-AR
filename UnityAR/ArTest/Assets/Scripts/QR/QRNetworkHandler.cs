using System.Collections;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class QRNetworkHandler : MonoBehaviour
{
    QRScanner m_QRScanner;
    GameObject m_QRModelDownloadScreen;
    StateMachine m_StateMachine;

    private void Start()
    {
        m_QRScanner = GameObject.FindWithTag("QRScanner").GetComponent<QRScanner>();
        m_QRModelDownloadScreen = GameObject.FindWithTag("QRModelDownloadScreen");
        m_QRModelDownloadScreen.SetActive(false);
        m_StateMachine = GameObject.FindWithTag("StateMachine").GetComponent<StateMachine>();
    }

    public void StartDownloadingModel()
    {
        StartCoroutine(DownloadModel(m_QRScanner.QrCode));
    }

    private void OnDownloadProgress(UnityWebRequest www)
    {
        m_QRModelDownloadScreen.GetComponentInChildren<Slider>().value = www.downloadProgress;
    }

    private IEnumerator DownloadModel(string uri)
    {
        m_QRModelDownloadScreen.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
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
        m_QRModelDownloadScreen.SetActive(false);
        m_StateMachine.SetState(StateMachine.States.IDLE_STATE);
    }
}
