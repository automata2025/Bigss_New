using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SaveData CurrentSave { get; private set; }

    private static bool _loaded;

    // ---------------------------------------------------------
    // BASIC LOAD / SAVE
    // ---------------------------------------------------------
    public static void Load()
    {
        if (_loaded && CurrentSave != null)
            return;

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);

            if (CurrentSave == null)
            {
                Debug.LogWarning("[SaveSystem] Save file was broken, creating new save.");
                CurrentSave = CreateNewSave();
            }
        }
        else
        {
            Debug.Log("[SaveSystem] No save file found, creating new save.");
            CurrentSave = CreateNewSave();
            Save(); // write first file
        }

        _loaded = true;
    }

    public static void Save()
    {
        if (CurrentSave == null)
            CurrentSave = CreateNewSave();

        string json = JsonUtility.ToJson(CurrentSave, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    // ---------------------------------------------------------
    // LEVEL HELPERS
    // ---------------------------------------------------------

    /// <summary>
    /// Save the result for this level.
    /// levelIndex is 1-based (Level1 = 1, Level2 = 2, etc).
    /// </summary>
    public static void SaveLevelResult(string levelId, int levelIndex, float clearTime, int stars)
    {
        Load();

        // Ensure positive index
        levelIndex = Mathf.Max(1, levelIndex);

        bool previouslyCleared = false;

        // Get or create the level entry
        LevelSaveData level = CurrentSave.GetOrCreateLevel(levelId, levelIndex);
        if (level != null)
        {
            previouslyCleared = level.cleared;
            level.ApplyResult(clearTime, stars);
        }

        // Unlock next level if this is newly cleared and matches progression
        if (!previouslyCleared && level != null && level.cleared)
        {
            // Example rule: if you beat level N and it's the highest,
            // unlock levelIndex+1
            if (levelIndex >= CurrentSave.lastUnlockedLevelIndex)
            {
                CurrentSave.lastUnlockedLevelIndex = levelIndex + 1;
            }
        }

        Save();
    }

    /// <summary>
    /// Find level data by id (may return null if not present yet).
    /// </summary>
    public static LevelSaveData GetLevelData(string levelId)
    {
        Load();
        return CurrentSave.FindLevel(levelId);
    }

    /// <summary>
    /// Is a given level index (1-based) unlocked?
    /// </summary>
    public static bool IsLevelUnlocked(int levelIndex)
    {
        Load();
        return levelIndex <= CurrentSave.lastUnlockedLevelIndex;
    }

    // ---------------------------------------------------------
    // INTERNAL
    // ---------------------------------------------------------

    private static SaveData CreateNewSave()
    {
        var data = new SaveData();

        // Level 1 unlocked by default
        data.lastUnlockedLevelIndex = 1;

        // Pre-create 4 levels (you can change this number)
        for (int i = 0; i < 4; i++)
        {
            string id = $"Level{i + 1}";
            int index = i + 1;
            data.levels.Add(new LevelSaveData(id, index));
        }

        return data;
    }
}
