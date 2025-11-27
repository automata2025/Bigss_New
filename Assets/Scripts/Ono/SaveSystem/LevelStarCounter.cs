using UnityEngine;

public class LevelStarCounter : MonoBehaviour
{
    public int maxStars = 3;
    public GameObject[] starObject;

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
        if (starObject == null) return;

        for (int i = 0; i < starObject.Length; i++)
        {
            if (starObject[i] != null)
            {
                starObject[i].SetActive(i < CurrentStars);
            }
        }
    }
}
