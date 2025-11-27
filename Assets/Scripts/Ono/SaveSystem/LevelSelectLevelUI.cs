using UnityEngine;
using UnityEngine.UI;

public class LevelSelectLevelUI : MonoBehaviour
{
    [Header("Level Info")]
    public string levelId = "Level1";
    public int levelIndex = 1;  

    [Header("UI References")]
    public GameObject lockOverlay;   
    public GameObject[] starIcons;   
    public Text timeText;            

    public void Apply(LevelSaveData data, bool isUnlocked)
    {
        // Lock overlay
        if (lockOverlay != null)
            lockOverlay.SetActive(!isUnlocked);

        // Stars
        int stars = (data != null) ? data.bestStars : 0;
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].SetActive(i < stars);
            }
        }

        // Time text
        if (timeText != null)
        {
            if (data != null && data.cleared && data.bestTime > 0f)
            {
                var t = System.TimeSpan.FromSeconds(data.bestTime);
                timeText.text = $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
            }
            else
            {
                timeText.text = "--:--";
            }
        }
    }
}
