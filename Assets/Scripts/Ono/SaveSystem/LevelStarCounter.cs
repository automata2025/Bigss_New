using UnityEngine;

public class LevelStarCounter : MonoBehaviour
{
    public int maxStars = 3;

    public GameObject[] starIcons; // size = 3

    public int CurrentStars { get; private set; }

    private void Start()
    {
        CurrentStars = 0;
        UpdateHud();
    }

    public void AddStar()
    {
        if (CurrentStars >= maxStars)
            return;

        CurrentStars++;
        UpdateHud();
        Debug.Log($"[LevelStarCounter] Stars collected: {CurrentStars}/{maxStars}");
    }

    private void UpdateHud()
    {
        if (starIcons == null) return;

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] != null)
            {
                starIcons[i].SetActive(i < CurrentStars);
            }
        }
    }
}
