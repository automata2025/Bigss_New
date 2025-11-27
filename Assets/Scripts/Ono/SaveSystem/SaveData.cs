using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelSaveData
{
    public string levelId;      
    public int levelIndex;     

    public bool cleared;
    public float bestTime; 
    public int bestStars;      

    public LevelSaveData(string id, int index)
    {
        levelId = id;
        levelIndex = index;
        cleared = false;
        bestTime = 0f;
        bestStars = 0;
    }

    public bool ApplyResult(float clearTime, int stars)
    {
        bool changed = false;

        cleared = true;

        if (bestTime <= 0f || clearTime < bestTime)
        {
            bestTime = clearTime;
            changed = true;
        }

        if (stars > bestStars)
        {
            bestStars = stars;
            changed = true;
        }

        return changed;
    }
}

[Serializable]
public class SaveData
{
    public int lastUnlockedLevelIndex = 1;  

    public List<LevelSaveData> levels = new List<LevelSaveData>();

    public LevelSaveData GetOrCreateLevel(string levelId, int levelIndex)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].levelId == levelId)
                return levels[i];
        }

        var data = new LevelSaveData(levelId, levelIndex);
        levels.Add(data);
        return data;
    }

    public LevelSaveData FindLevel(string levelId)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].levelId == levelId)
                return levels[i];
        }
        return null;
    }
}
