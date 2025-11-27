using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectLevelUI : MonoBehaviour
{
    public string levelId = "Level1";

    public int levelIndex = 1;

    public TextMeshProUGUI bestTimeText;

    public GameObject[] starIcons;

    public GameObject lockOverlay;

  
    public void Apply(LevelSaveData data, bool isUnlocked)
    {
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked);
        }

        int stars = (data != null) ? data.bestStars : 0;
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(i < stars);
                }
            }
        }

        if (bestTimeText != null)
        {
            if (data != null && data.cleared && data.bestTime > 0f)
            {
                var t = System.TimeSpan.FromSeconds(data.bestTime);
                bestTimeText.text = $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
            }
            else
            {
                bestTimeText.text = "--:--";
            }
        }
    }
}
