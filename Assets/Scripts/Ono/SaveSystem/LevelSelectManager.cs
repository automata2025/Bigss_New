using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void OnNewGameClicked()
    {
        Debug.Log("[LevelSelectManager] New Game clicked.");
        SaveSystem.ClearSave();

        SaveSystem.Load();

        var save = SaveSystem.CurrentSave;
        if (save == null)
        {
            Debug.LogError("[LevelSelectManager] After ClearSave, CurrentSave is null!");
            return;
        }

        Debug.Log($"[LevelSelectManager] After ClearSave: lastUnlockedLevelIndex = {save.lastUnlockedLevelIndex}");

        if (save.levels != null)
        {
            foreach (var lvl in save.levels)
            {
                Debug.Log(
                    $"[LevelSelectManager] Level {lvl.levelId} (index {lvl.levelIndex}) " +
                    $"cleared={lvl.cleared}, bestTime={lvl.bestTime}, bestStars={lvl.bestStars}"
                );
            }
        }
        RefreshUI();
    }
}
