using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private int levelIndex = 1; // set this in Inspector (1–4)

    public void LoadLevel()
    {
        SceneManager.LoadScene(levelIndex);
    }
}

