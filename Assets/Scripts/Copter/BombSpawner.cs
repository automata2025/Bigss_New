using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public BombPool bombPool;
    public Transform spawnPoint;
    public float spawnHeight = 50f;
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            DropBomb();
        }
    }

    public void DropBomb()
    {
        Vector3 spawnPos = spawnPoint ?
            spawnPoint.position + Vector3.up * spawnHeight :
            transform.position + Vector3.up * spawnHeight;

        bombPool.GetBomb(spawnPos, Quaternion.identity);
    }
}
