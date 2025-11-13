using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public BombPool bombPool;
    public Transform spawnPoint;
    public float spawnHeight = 50f;
    public float spawnInterval = 2f;
    public float spacing = 3f; // distance between each bomb in the line

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            Drop3BombsInFormation();
        }
    }

    void Drop3BombsInFormation()
    {
        Vector3 basePos = spawnPoint
            ? spawnPoint.position + Vector3.up * spawnHeight
            : transform.position + Vector3.up * spawnHeight;

        // Spawn left, center, and right bombs
        Vector3 left = basePos + transform.right * -spacing;
        Vector3 center = basePos;
        Vector3 right = basePos + transform.right * spacing;

        bombPool.GetBomb(left, Quaternion.identity);
        bombPool.GetBomb(center, Quaternion.identity);
        bombPool.GetBomb(right, Quaternion.identity);
    }
}
