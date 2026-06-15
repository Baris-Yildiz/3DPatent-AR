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

/// <summary>
/// Singleton that downloads a glTF/GLB model from the URL encoded in the
/// scanned QR code and loads it into the scene using GLTFast. Shows a
/// progress-bar popup during download and an error popup on failure.
/// On success it sets <see cref="PatentManager.Patent"/> and transitions to
/// <see cref="StateMachine.States.MODEL_VIEW_STATE"/>.
/// </summary>
public class QRNetworkHandler : MonoBehaviour
{
    GameObject m_QRModelDownloadScreen;
    StateMachine m_StateMachine;

    /// <summary>Temporary reference to the loaded glTF scene root, cleared after cloning to <see cref="PatentManager.Patent"/>.</summary>
    public GameObject LoadedModel;

    private ObjectSpawnerAR _objectSpawnerAR;

    private const string ProgressBarPath = "Progress Bar";
    private GameObject m_ProgressBarPrefab;

    /// <summary>Transform under which the GLTFast importer instantiates the model temporarily.</summary>
    public Transform ModelLoadTransform;

    /// <summary>The single active instance of <see cref="QRNetworkHandler"/>.</summary>
    public static QRNetworkHandler Instance
    {
        get; private set;
    }

    /// <summary>Initialises the singleton instance.</summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Sets up the model load transform, wires state-machine callbacks to trigger
    /// download and hide the progress screen, and pre-loads the progress bar prefab.
    /// </summary>
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

    /// <summary>
    /// Shows the download-progress popup, creating it on first call.
    /// </summary>
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

    /// <summary>
    /// Resets handler state and starts downloading the model whose URL was
    /// decoded by <see cref="QRScanner"/>.
    /// </summary>
    public void StartDownloadingModel()
    {
        ResetNetworkHandler();
        StartCoroutine(DownloadModel(QRScanner.Instance.QrCode));
    }

    /// <summary>
    /// Stops all download coroutines, returns to <see cref="StateMachine.States.NO_MODEL_VIEW_STATE"/>,
    /// and destroys the download popup. The popup is destroyed (not hidden) because
    /// either a progress or error variant may exist.
    /// </summary>
    private void CancelDownload()
    {
        StopAllCoroutines();
        StateMachine.Instance.SetState(StateMachine.States.NO_MODEL_VIEW_STATE);
        Destroy(m_QRModelDownloadScreen);
    }

    /// <summary>
    /// Updates the progress bar slider and percentage label during an active download.
    /// </summary>
    /// <param name="progress">Download progress in the range 0–1.</param>
    private void OnDownloadProgress(float progress)
    {
        m_QRModelDownloadScreen.GetComponentInChildren<QRProgressBarManager>().Slider.value = progress;
        m_QRModelDownloadScreen.GetComponentInChildren<QRProgressBarManager>().ProgressText.text = string.Format("{0:F2}%", progress * 100f);
    }

    /// <summary>Clears the <see cref="LoadedModel"/> reference to prepare for a new download.</summary>
    private void ResetNetworkHandler()
    {
        LoadedModel = null;
    }

    /// <summary>
    /// Asynchronously parses raw GLB bytes with GLTFast, instantiates the scene
    /// under this transform, clones it as the <see cref="PatentManager.Patent"/>
    /// prefab, adds <see cref="GenerateLOD"/>, then cleans up the temporary instance.
    /// </summary>
    /// <param name="data">Raw GLB file bytes returned by the web request.</param>
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

    /// <summary>
    /// Destroys the progress popup and replaces it with an error popup that
    /// displays the failure message and an OK button to dismiss.
    /// </summary>
    /// <param name="error">The error message from the failed web request.</param>
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

    /// <summary>
    /// Coroutine that downloads the model at <paramref name="uri"/>, streams progress
    /// to the UI, writes the bytes to persistent storage, then calls
    /// <see cref="LoadGLBObject"/> before transitioning to
    /// <see cref="StateMachine.States.MODEL_VIEW_STATE"/>.
    /// </summary>
    /// <param name="uri">The URL of the GLB file to download.</param>
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
