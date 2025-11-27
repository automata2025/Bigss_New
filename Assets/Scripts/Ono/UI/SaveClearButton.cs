using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveClearButton : MonoBehaviour
{
    public bool reloadScene = true;

    public void OnClearSaveClicked()
    {
        SaveSystem.ClearSave();

        if (reloadScene)
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
        else
        {
            var mgr = FindObjectOfType<LevelSelectManager>();
            if (mgr != null)
            {
                mgr.RefreshUI();
            }
        }
    }
}

