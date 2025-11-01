using System;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class SceneManagerAr : MonoBehaviour
{

    public static SceneManagerAr instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
        
        DontDestroyOnLoad(this);
    }


    public void GoToNextScene()
    {
        
        Scene currScene = SceneManager.GetActiveScene();
        Debug.Log(currScene.name);
        if (currScene.name == "QR Code Scanning")
        {
           
            SceneManager.LoadScene("MyArScene" , LoadSceneMode.Single);
        }
        else if(currScene.name == "MyArScene")
        {
            SceneManager.LoadScene("QR Code Scanning" , LoadSceneMode.Single);
        }
    }
}
