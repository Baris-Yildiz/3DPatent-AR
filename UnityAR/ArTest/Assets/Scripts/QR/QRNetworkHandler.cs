using GLTFast;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using TMPro;
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
    GameObject m_LoadedModel;

    private void Start()
    {
        m_QRScanner = GameObject.FindWithTag("QRScanner").GetComponent<QRScanner>();
        m_QRModelDownloadScreen = GameObject.FindWithTag("QRModelDownloadScreen");
        m_QRModelDownloadScreen.SetActive(false);
        m_StateMachine = GameObject.FindWithTag("StateMachine").GetComponent<StateMachine>();
        m_LoadedModel = null;
    }

    public void StartDownloadingModel()
    {
        ResetNetworkHandler();
        StartCoroutine(DownloadModel(m_QRScanner.QrCode));
    }

    private void OnDownloadProgress(UnityWebRequest www)
    {
        m_QRModelDownloadScreen.GetComponentInChildren<Slider>().value = www.downloadProgress;
    }

    private void ResetNetworkHandler()
    {
        m_LoadedModel = null;
    }

    private async Task LoadGLBObject(byte[] data)
    {
        var gltf = new GltfImport();
        m_QRModelDownloadScreen.GetComponentInChildren<TextMeshProUGUI>().text = "Loading Model";
        bool success = await gltf.Load(data);

        if (success)
        {
            Transform spawnParent = GameObject.Find("QR").transform; //as an example, download the model onto the qr code
            success = await gltf.InstantiateMainSceneAsync(spawnParent);
            if (success)
            {
                Debug.Log("Model loaded successfully!");
                m_LoadedModel = spawnParent.GetChild(0).gameObject;
                return;
            }
        }

        Debug.LogError("Model load failed");
    }

    private IEnumerator DownloadModel(string uri)
    {
        m_QRModelDownloadScreen.SetActive(true);
        m_QRModelDownloadScreen.GetComponentInChildren<TextMeshProUGUI>().text = "Downloading Model";
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
                Task loadTask = LoadGLBObject(www.downloadHandler.data);
                yield return loadTask.IsCompleted;
            }
        }
        
        m_QRModelDownloadScreen.SetActive(false);
        m_StateMachine.SetState(StateMachine.States.IDLE_STATE);
    }
}
