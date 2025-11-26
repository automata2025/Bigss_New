using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    public float BombSpacing = 5f;
    public int Rows = 3;
    public int Cols = 3;

    [Header("Drop Settings")]
    [SerializeField] private float dropHeight = 20f;

    [Header("References")]
    [SerializeField] private BombPool bombPool;

    private void Start()
    {
        // Drop immediately once
        DropBombGrid(new Vector3(transform.position.x, 0f, transform.position.z));
    }

    private void DropBombGrid(Vector3 center)
    {
        float startX = center.x - ((Cols - 1) * BombSpacing) / 2f;
        float startZ = center.z - ((Rows - 1) * BombSpacing) / 2f;

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Vector3 pos = new Vector3(
                    startX + c * BombSpacing,
                    dropHeight,
                    startZ + r * BombSpacing
                );

                bombPool.GetBomb(pos, Quaternion.identity);
            }
        }
    }
}
