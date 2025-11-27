using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private ButtonAction sceneActions;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }


    public void OnRestartButton()
    {
        sceneActions?.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        sceneActions?.LoadMainMenu();
    }

    public void OnQuitButton()
    {
        sceneActions?.QuitGame();
    }
}
