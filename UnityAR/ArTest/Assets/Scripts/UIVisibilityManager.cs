using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisibilityManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    List<GameObject> m_ObjectsToModifyVisibility;
    private UIToggle m_UIToggleComponent;

    void Start()
    {
        m_UIToggleComponent = GetComponent<UIToggle>();

        GameObject modelViewObject = ModelViewManager.Instance.gameObject;
        GameObject modelTransformObject = ModelTransformManager.Instance.gameObject;
        GameObject miscOptionsObject = ModelPlaneDetectionManager.Instance.gameObject.transform.parent.gameObject;

        GameObject modelPlaneDetectionObject = ModelPlaneDetectionManager.Instance.gameObject;

        m_ObjectsToModifyVisibility = new List<GameObject> { 
            modelViewObject, 
            modelTransformObject, 
            modelPlaneDetectionObject 
        };

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () =>
            {                
                modelViewObject.SetActive(false);
                modelTransformObject.SetActive(false);
                miscOptionsObject.SetActive(false);
            };

        StateMachine.Instance.GetState(StateMachine.States.MODEL_VIEW_STATE)
            .OnStateEnter += () =>
            {
                modelViewObject.SetActive(true);
                modelTransformObject.SetActive(true);
                miscOptionsObject.SetActive(true);
                GetComponent<Toggle>().isOn = false;
            };

        m_UIToggleComponent.OnToggleValueChanged += (bool isOn) => { SetUIObjectVisibility(isOn); };
    }

    private void SetUIObjectVisibility(bool isOn)
    {
        foreach (GameObject obj in m_ObjectsToModifyVisibility)
        {
            obj.SetActive(!isOn);
        }
    }

}
