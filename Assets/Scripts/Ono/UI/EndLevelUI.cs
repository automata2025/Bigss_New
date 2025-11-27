using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndLevelUI : MonoBehaviour
{
    public GameObject rootPanel;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI starsText;

    public ButtonAction gameFlow;

    private bool _shown;

    private void Start()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Show(float clearTimeSeconds, int stars)
    {
        if (_shown) return;
        _shown = true;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        Time.timeScale = 0f;

        if (timeText != null)
        {
            var t = System.TimeSpan.FromSeconds(clearTimeSeconds);
            timeText.text = $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
        }

        // Stars
        if (starsText != null)
        {
            starsText.text = $"{stars} / 3";
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        if (gameFlow != null) gameFlow.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        if (gameFlow != null) gameFlow.LoadMainMenu();
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        if (gameFlow != null) gameFlow.QuitGame();
    }

    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        if (gameFlow != null) gameFlow.LoadNextLevel();
    }
}
