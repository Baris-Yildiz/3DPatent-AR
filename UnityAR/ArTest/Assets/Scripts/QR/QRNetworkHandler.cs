using GLTFast;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class QRNetworkHandler : MonoBehaviour
{
    GameObject m_QRModelDownloadScreen;
    StateMachine m_StateMachine;
    public GameObject LoadedModel;
    private ObjectSpawnerAR _objectSpawnerAR;

    private const string ProgressBarPath = "Progress Bar";
    private GameObject m_ProgressBar;

    public Transform ModelLoadTransform;

    public static QRNetworkHandler Instance
    {
        get; private set;
    }

    public void DisplayModelDownloadScreen()
    {
        if (m_QRModelDownloadScreen != null)
        {
            m_QRModelDownloadScreen.SetActive(true);
            return;
        }

        PopupBuilder builder = PopupBuilder.Create();

        m_QRModelDownloadScreen = builder
            .WithHeader("MODEL ÝNDÝRÝLÝYOR")
            .WithFooterButton("Ýptal Et", PopupWindow.BUTTON_COLOR_CANCEL, CancelDownload)
            .WithContent(m_ProgressBar)
            .Get();
    }

    private void SetToQRScanState()
    {
        StateMachine.Instance.SetState(StateMachine.States.QR_SCAN_STATE);
    }

    private void CancelDownload()
    {
        StopAllCoroutines();
        Invoke(nameof(SetToQRScanState), 5f);
        m_QRModelDownloadScreen.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ModelLoadTransform = transform;
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE).
            OnStateEnter += () => StartDownloadingModel();

        _objectSpawnerAR = FindAnyObjectByType<ObjectSpawnerAR>();
        LoadedModel = null;

        m_ProgressBar = Instantiate( Resources.Load<GameObject>(ProgressBarPath) );
        m_ProgressBar.SetActive(false);
    }

    public void StartDownloadingModel()
    {
        ResetNetworkHandler();
        StartCoroutine(DownloadModel(QRScanner.Instance.QrCode));
    }

    private void OnDownloadProgress(float progress)
    {
        m_QRModelDownloadScreen.GetComponentInChildren<QRProgressBarManager>().Slider.value = progress;
        m_QRModelDownloadScreen.GetComponentInChildren<QRProgressBarManager>().ProgressText.text = string.Format("{0:F2}%", progress * 100f);
    }

    private void ResetNetworkHandler()
    {
        LoadedModel = null;
    }

    private async Task LoadGLBObject(byte[] data)
    {
        var gltf = new GltfImport();
        bool success = await gltf.Load(data);

        if (success)
        {
            Transform spawnParent = transform; //as an example, download the model onto the qr code
            success = await gltf.InstantiateMainSceneAsync(spawnParent);
            if (success)
            {
                Debug.Log("Model loaded successfully!");
                LoadedModel = spawnParent.GetChild(0).gameObject;
                LoadedModel.transform.rotation = Quaternion.identity;
                _objectSpawnerAR.SetActivePatent(LoadedModel);
                return;
            }
        }

        Debug.LogError("Model load failed");
    }

    private IEnumerator DownloadModel(string uri)
    {
        DisplayModelDownloadScreen();
        
        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            www.SendWebRequest();
            
            while (www.downloadProgress < 1)
            {
                OnDownloadProgress(www.downloadProgress);
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

                while (!loadTask.IsCompleted)
                {
                    yield return null;
                }

            }
        }
        
        m_QRModelDownloadScreen.SetActive(false);
        StateMachine.Instance.SetState(StateMachine.States.IDLE_STATE);
    }
}
