using UnityEngine;

public class ModelPlaneDetectionManager : MonoBehaviour
{
    GameObject m_Parent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Parent = gameObject.transform.parent.gameObject;
        m_Parent.SetActive(false);

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateExit += () => { m_Parent.SetActive(true); };

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => { m_Parent.SetActive(false); };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
