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

    [Header("Timing Settings")]
    [SerializeField] private float offDuration = 2f;     // Time before bombs drop
    [SerializeField] private float dropInterval = 4f;    // Time after bombs drop before dropping again

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

    public float OffDuration
    {
        get => offDuration;
        set => offDuration = Mathf.Max(0f, value);
    }

    public float DropInterval
    {
        get => dropInterval;
        set => dropInterval = Mathf.Max(0.1f, value);
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
            // WAIT before dropping (matches indicator "OFF" time)
            yield return new WaitForSeconds(offDuration);

            // DROP bombs
            DropBombGrid(new Vector3(
                transform.position.x,
                0f,
                transform.position.z
            ));

            // WAIT after dropping (matches indicator "BLINK" time)
            yield return new WaitForSeconds(dropInterval);
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
