using UnityEngine;
using System.Collections;

public class BombSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float bombSpacing = 5f;
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 3;

    [Header("Drop Settings")]
    [SerializeField] private float dropHeight = 20f;

    [Header("Interval Settings")]
    [SerializeField] private float dropInterval = 7f;

    [Header("References")]
    [SerializeField] private BombPool bombPool;

    // ------- Properties -------
    public float BombSpacing
    {
        get => bombSpacing;
        set => bombSpacing = value;
    }

    public int Rows
    {
        get => rows;
        set => rows = value;
    }

    public int Cols
    {
        get => cols;
        set => cols = value;
    }

    public float DropHeight
    {
        get => dropHeight;
        set => dropHeight = value;
    }

    public float DropInterval
    {
        get => dropInterval;
        set => dropInterval = Mathf.Max(0.1f, value); // prevent 0
    }

    public BombPool BombPool
    {
        get => bombPool;
        set => bombPool = value;
    }

    // ------- Logic -------
    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(dropInterval);

            DropBombGrid(new Vector3(
                transform.position.x,
                0f,
                transform.position.z
            ));
        }
    }

    private void DropBombGrid(Vector3 center)
    {
        float startX = center.x - ((cols - 1) * bombSpacing) / 2f;
        float startZ = center.z - ((rows - 1) * bombSpacing) / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(
                    startX + c * bombSpacing,
                    dropHeight,
                    startZ + r * bombSpacing
                );

                bombPool.GetBomb(pos, Quaternion.identity);
            }
        }
    }
}
