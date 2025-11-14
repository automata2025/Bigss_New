using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData CurrentSave { get; private set; }

    // Call this when the game starts (e.g. from a GameManager)
    public static void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);

            // Safety check
            if (CurrentSave == null)
            {
                Debug.LogWarning("Save file was empty/broken, creating new save.");
                CurrentSave = CreateNewSave();
            }
        }
        else
        {
            Debug.Log("No save file found, creating new save.");
            CurrentSave = CreateNewSave();
            Save(); // optional: create the file immediately
        }

        Debug.Log($"Loaded save. debugScore = {CurrentSave.debugScore}");
    }

    public static void Save()
    {
        if (CurrentSave == null)
            CurrentSave = CreateNewSave();

        string json = JsonUtility.ToJson(CurrentSave, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Saved save.json at {SavePath}. debugScore = {CurrentSave.debugScore}");
    }

    private static SaveData CreateNewSave()
    {
        return new SaveData
        {
            debugScore = 0
        };
    }
}
