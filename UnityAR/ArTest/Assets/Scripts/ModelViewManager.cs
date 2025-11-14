using UnityEngine;
using UnityEngine.UI;

public class ModelViewManager : MonoBehaviour
{
    public Button NormalViewButton;
    public Button OneToOneViewButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_COMPLETE_STATE)
            .OnStateExit += () => { gameObject.SetActive(true); };

        StateMachine.Instance.GetState(StateMachine.States.QR_SCAN_STATE)
            .OnStateEnter += () => { gameObject.SetActive(false); };

        NormalViewButton.onClick.AddListener(SetNormalView);
        OneToOneViewButton.onClick.AddListener(SetOneToOneView);

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetNormalView()
    {

    }

    private void SetOneToOneView()
    {

    }
}
