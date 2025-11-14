using UnityEngine;

public class DebugSaveTester : MonoBehaviour
{
    private void Awake()
    {
        // Make sure we load at game start
        SaveSystem.Load();
    }

    private void Update()
    {
        // Press R to generate a random score and save
        if (Input.GetKeyDown(KeyCode.R))
        {
            int randomScore = Random.Range(0, 1000);
            SaveSystem.CurrentSave.debugScore = randomScore;
            SaveSystem.Save();

            Debug.Log($"[DebugSaveTester] Random score generated and saved: {randomScore}");
        }

        // Press L to reload from file and print value
        if (Input.GetKeyDown(KeyCode.L))
        {
            SaveSystem.Load();
            Debug.Log($"[DebugSaveTester] Loaded score: {SaveSystem.CurrentSave.debugScore}");
        }
    }
}
