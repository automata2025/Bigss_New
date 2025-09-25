using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadWinOnCollision : MonoBehaviour
{
    [SerializeField] private string winSceneName = "WinScene"; // set your scene name in Inspector

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }
}
