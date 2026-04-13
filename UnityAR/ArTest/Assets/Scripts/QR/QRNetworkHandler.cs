using GLTFast;
using GLTFast.Logging;
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
    private GameObject m_ProgressBarPrefab;

    public Transform ModelLoadTransform;

    public static QRNetworkHandler Instance
    {
        get; private set;
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

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE).
            OnStateEnter += () => { if (m_QRModelDownloadScreen != null) m_QRModelDownloadScreen.SetActive(false); };

        StateMachine.Instance.GetState(StateMachine.States.NO_MODEL_VIEW_STATE).
            OnStateEnter += () => { if (m_QRModelDownloadScreen != null) m_QRModelDownloadScreen.SetActive(false); };

        _objectSpawnerAR = FindAnyObjectByType<ObjectSpawnerAR>();
        LoadedModel = null;

        m_ProgressBarPrefab = Resources.Load<GameObject>(ProgressBarPath);
        
    }

    public void DisplayModelDownloadScreen()
    {
        if (m_QRModelDownloadScreen != null)
        {
            m_QRModelDownloadScreen.SetActive(true);
            return;
        }

        PopupBuilder builder = PopupBuilder.Create();

        GameObject progressBar = Instantiate(m_ProgressBarPrefab);
        progressBar.SetActive(false);

        m_QRModelDownloadScreen = builder
            .WithHeader("DOWNLOADING MODEL")
            .WithFooterButton("Cancel", PopupWindow.BUTTON_COLOR_CANCEL, CancelDownload)
            .WithContent(progressBar)
            .Get();
    }

    public void StartDownloadingModel()
    {
        ResetNetworkHandler();
        StartCoroutine(DownloadModel(QRScanner.Instance.QrCode));
    }

    private void CancelDownload()
    {
        StopAllCoroutines();
        StateMachine.Instance.SetState(StateMachine.States.NO_MODEL_VIEW_STATE);
        Destroy(m_QRModelDownloadScreen); //destroy because there are two types of popups: error and progress
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
        var logger = new CollectingLogger();
        var gltf = new GltfImport(logger: logger);
        bool success = await gltf.Load(data);

        if (success)
        {
            Transform spawnParent = transform; //as an example, download the model onto the qr code
            success = await gltf.InstantiateMainSceneAsync(spawnParent);
            if (success)
            {
                Debug.Log("Model loaded successfully!");
                LoadedModel = spawnParent.GetChild(0).gameObject;
               // LoadedModel.transform.rotation = Quaternion.identity;
                
                PatentManager.Instance.Patent = Instantiate(LoadedModel);
                PatentManager.Instance.Patent.SetActive(false);
                PatentManager.Instance.Patent.AddComponent<GenerateLOD>();
                Debug.Log("patent setted to object ");
                LoadedModel = null;
                Destroy(spawnParent.GetChild(0).gameObject);
                //_objectSpawnerAR.SetActivePatent(LoadedModel);
                
                return;
            }
        }

        if (logger.Items != null)
        {
            foreach (var item in logger.Items)
            {
                Debug.LogError(item.ToString());
            }
        }

        Debug.LogError("Model load failed");
    }

    private void OnDownloadError(string error)
    {
        Debug.LogError("Error: " + error);

        Destroy(m_QRModelDownloadScreen);

        PopupBuilder builder = PopupBuilder.Create();

        m_QRModelDownloadScreen = builder
            .WithHeader("ERROR")
            .WithFooterButton("OK", PopupWindow.BUTTON_COLOR_OK, CancelDownload)
            .WithContentText(string.Format("An error occured: {0}", error))
            .Get();
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
                OnDownloadError(www.error);
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
                StateMachine.Instance.SetState(StateMachine.States.MODEL_VIEW_STATE);
            }
        }        
    }
}
