using UnityEngine;
using UnityEngine.UI;

public class ModelTransformManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Button LockModelButton;
    public Button ResetModelTransformButton;
    public Button DeleteModelTransformButton;

    void Start()
    {
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateExit += () => { gameObject.SetActive(true); };

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => { gameObject.SetActive(false); };

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
