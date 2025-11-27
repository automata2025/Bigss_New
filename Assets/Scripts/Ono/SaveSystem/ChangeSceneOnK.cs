using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneOnK : MonoBehaviour
{
    // Set this in the Inspector to the name of the scene you want to load
    public string sceneToLoad;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("sceneToLoad is empty on ChangeSceneOnK!");
            }
        }
    }
}

