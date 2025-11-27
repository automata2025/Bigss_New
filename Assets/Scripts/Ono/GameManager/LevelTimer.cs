using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private bool autoStart = true;
    [SerializeField] private TextMeshProUGUI timeText;

    public bool IsRunning { get; private set; }
    public float ElapsedTime { get; private set; }

    private void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }

        if (timeText != null)
        {
            timeText.text = FormatTime(ElapsedTime);
        }
    }

    private void Update()
    {
        if (!IsRunning) return;

        ElapsedTime += Time.deltaTime;

        if (timeText != null)
        {
            timeText.text = FormatTime(ElapsedTime);
        }
    }


    public void StartTimer()
    {
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public void ResetTimer()
    {
        ElapsedTime = 0f;
        if (timeText != null)
        {
            timeText.text = FormatTime(ElapsedTime);
        }
    }

    public string GetFormattedTime()
    {
        return FormatTime(ElapsedTime);
    }

    private string FormatTime(float seconds)
    {
        var t = System.TimeSpan.FromSeconds(seconds);
        return $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
    }
}

