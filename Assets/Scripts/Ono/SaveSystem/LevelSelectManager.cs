using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public LevelSelectLevelUI[] levelPanels;

    private void Awake()
    {
        SaveSystem.Load();
        RefreshUI();
    }

    private void OnEnable()
    {
        SaveSystem.Load();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (levelPanels == null || levelPanels.Length == 0)
            return;

        for (int i = 0; i < levelPanels.Length; i++)
        {
            var panel = levelPanels[i];
            if (panel == null) continue;

            LevelSaveData data = SaveSystem.GetLevelData(panel.levelId);
            bool unlocked = SaveSystem.IsLevelUnlocked(panel.levelIndex);

            panel.Apply(data, unlocked);
        }
    }
}
