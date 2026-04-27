using System;
using TMPro;
using UnityEngine;

public class LogText : MonoBehaviour
{
    private TextMeshProUGUI text;
    [SerializeField] private float dropSpeed = 0.2f;
    [SerializeField] private float stayDuration = 2f;
    [SerializeField] private float fadeSpeedMultiplier;
    
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color normalColor  = Color.black;

    [SerializeField] private float endPointThreshold = 0.1f;
    private Vector2 startPoint;
    private Vector2 endPoint;

    private bool isStarted = false;
    private float fadeSpeed;
    private float remainingStayDuration;
    private Vector2 targetPos;

    private Color currentColor;
    public void StartLog(Vector2 _startPoint, Vector2 _endPoint)
    {
        startPoint = _startPoint;
        endPoint = _endPoint;
        isStarted = true;
        fadeSpeed = Mathf.Abs(_endPoint.y - _startPoint.y)/dropSpeed;
        remainingStayDuration = stayDuration;
    }

    public void SetText(String _text , bool isError)
    {
        text.text = _text;
        Color color = isError ? errorColor : normalColor;
        color.a = 0;
        currentColor = color;
        text.color = color;
    }

    public void UpdateLog()
    {
        if (isStarted)
        {
            MoveLog();
        }
    }

    private void MoveLog()
    {
        if (Mathf.Abs(transform.localPosition.y - endPoint.y) <= endPointThreshold)
        {
            remainingStayDuration -= Time.deltaTime;
            if (remainingStayDuration <= 0)
            {
                isStarted = false;
                LogManager.Instance.EnqueueLog(this);   
            }
        }
        else
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition , endPoint , dropSpeed*Time.deltaTime );
        }
        currentColor.a = Mathf.Min(1, currentColor.a + fadeSpeed * Time.deltaTime);
        text.color = currentColor;
    }

}
