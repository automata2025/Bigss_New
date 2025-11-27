using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public LevelSelectLevelUI[] levelPanels;

    private void Awake()
    {
        SaveSystem.Load();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (levelPanels == null || levelPanels.Length == 0)
            return;

        for (int i = 0; i < levelPanels.Length; i++)
        {
            var panel = levelPanels[i];
            if (panel == null) continue;

            // Get the saved data (may be null if never touched)
            LevelSaveData data = SaveSystem.GetLevelData(panel.levelId);

            // Unlock rule: level is unlocked if its index <= lastUnlockedLevelIndex
            bool unlocked = SaveSystem.IsLevelUnlocked(panel.levelIndex);

            panel.Apply(data, unlocked);
        }
    }
}
