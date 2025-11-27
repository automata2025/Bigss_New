using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SaveData CurrentSave { get; private set; }

    private static bool _loaded;



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
                Debug.LogWarning("[SaveSystem] Save file broken, creating new save.");
                CurrentSave = CreateNewSave();
            }
        }
        else
        {
            Debug.Log("[SaveSystem] No save file, creating new.");
            CurrentSave = CreateNewSave();
            Save();
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


    public static void SaveLevelResult(string levelId, int levelIndex, float clearTime, int stars)
    {
        Load();

        levelIndex = Mathf.Max(1, levelIndex);

        bool previouslyCleared = false;
        bool changed = false;

        LevelSaveData level = CurrentSave.GetOrCreateLevel(levelId, levelIndex);
        if (level != null)
        {
            previouslyCleared = level.cleared;
            // ApplyResult returns true only if bestTime/bestStars improved
            changed = level.ApplyResult(clearTime, stars);
        }

        // Unlock next level if this is newly cleared
        if (!previouslyCleared && level != null && level.cleared)
        {
            if (levelIndex >= CurrentSave.lastUnlockedLevelIndex)
            {
                CurrentSave.lastUnlockedLevelIndex = levelIndex + 1;
                changed = true; // progression changed
            }
        }

        if (changed)
        {
            Save();
        }
        else
        {
            Debug.Log("[SaveSystem] Run did not beat best score/time; save not updated.");
        }
    }

    public static LevelSaveData GetLevelData(string levelId)
    {
        Load();
        return CurrentSave.FindLevel(levelId);
    }

    public static bool IsLevelUnlocked(int levelIndex)
    {
        Load();
        return levelIndex <= CurrentSave.lastUnlockedLevelIndex;
    }

    public static void ClearSave()
    {
        // Delete file on disk if exists
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] Save file deleted.");
        }

        // Reset in-memory data
        CurrentSave = CreateNewSave();
        _loaded = true;

        // Immediately write a fresh save file
        Save();
    }

    private static SaveData CreateNewSave()
    {
        var data = new SaveData();

        // Start with only Level1 unlocked
        data.lastUnlockedLevelIndex = 1;

        // Pre-create 4 levels: Level1..Level4
        for (int i = 0; i < 4; i++)
        {
            string id = $"Level{i + 1}";
            int index = i + 1;
            data.levels.Add(new LevelSaveData(id, index));
        }

        return data;
    }
}
