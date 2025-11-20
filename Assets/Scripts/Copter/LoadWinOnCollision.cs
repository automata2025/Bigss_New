using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadWinOnCollision : MonoBehaviour
{
    [SerializeField] private string winSceneName = "WinScene"; // set your scene name in Inspector

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            UnlockNewLevel(); // says to unlocked
            SceneManager.LoadScene(winSceneName);
        }
    }

    // this void unlock new level
    void UnlockNewLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.Save();

        }
    }
}
