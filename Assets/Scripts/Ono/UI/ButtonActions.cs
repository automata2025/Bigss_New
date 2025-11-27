using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonAction : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void LoadMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[GameFlowController] MainMenu scene name is not set!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[GameFlowController] Quit Game");
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        Scene current = SceneManager.GetActiveScene();
        int nextIndex = current.buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            LoadMainMenu();
        }
    }
}
