using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [Header("References")]
    public LevelTimer timer;
    public LevelStarCounter starCounter;
    public LevelResultSaver resultSaver;

    [Header("Settings")]
    [Tooltip("Only objects with this tag can finish the level.")]
    public string playerTag = "Player";

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;

        // Stop timer
        float finalTime = 0f;
        if (timer != null)
        {
            timer.StopTimer();
            finalTime = timer.ElapsedTime;
        }

        // Get star count
        int stars = 0;
        if (starCounter != null)
        {
            stars = starCounter.CurrentStars;
        }

        // Save result
        if (resultSaver != null)
        {
            resultSaver.OnLevelCleared(finalTime, stars);
        }

        Debug.Log($"[LevelGoal] Level finished in {finalTime:F2} sec with {stars} star(s).");

        // TODO: load next scene / level select here if you want
        // SceneManager.LoadScene("LevelSelect");
    }
}
