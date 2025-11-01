using UnityEngine;

public class SceneTransitionButton : MonoBehaviour
{


    public void OnSceneChangeClicked()
    {
        GameManager.instance.ChangeMode();
    }
}
