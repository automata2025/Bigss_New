using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public LevelTimer timer;
    public LevelStarCounter starCounter;
    public LevelResultSaver resultSaver;
    public EndLevelUI endLevelUI;

    public string playerTag = "Player";

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;

        float finalTime = 0f;
        if (timer != null)
        {
            timer.StopTimer();
            finalTime = timer.ElapsedTime;
        }

        int stars = 0;
        if (starCounter != null)
        {
            stars = starCounter.CurrentStars;
        }

        if (resultSaver != null)
        {
            resultSaver.OnLevelCleared(finalTime, stars);
        }

        if (endLevelUI != null)
        {
            endLevelUI.Show(finalTime, stars);
        }
        else
        {
            Debug.LogWarning("[LevelGoal] EndLevelUI not assigned.");
        }
    }
}

