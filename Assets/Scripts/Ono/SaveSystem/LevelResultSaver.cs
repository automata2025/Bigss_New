using UnityEngine;

public class LevelResultSaver : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Must match the id used in SaveData/SaveSystem (e.g. Level1, Level2, ...)")]
    public string levelId = "Level1";

    [Tooltip("1-based index: Level1 = 1, Level2 = 2, ...")]
    public int levelIndex = 1;

    [Header("Stars")]
    [Tooltip("Maximum stars this level can give (usually 3).")]
    public int maxStars = 3;

    public void OnLevelCleared(float clearTime, int starsEarned)
    {
        starsEarned = Mathf.Clamp(starsEarned, 0, maxStars);

        SaveSystem.SaveLevelResult(levelId, levelIndex, clearTime, starsEarned);

        Debug.Log($"[LevelResultSaver] Saved {levelId} (index {levelIndex}) time={clearTime:F2}, stars={starsEarned}");
    }
}

