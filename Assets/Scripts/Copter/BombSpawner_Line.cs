using UnityEngine;
using System.Collections;

public class BombSpawner_Line : MonoBehaviour
{
    [Header("Row Settings")]
    [SerializeField] private int bombCount = 5;          // Always 5 bombs
    [SerializeField] private float bombSpacing = 5f;     // Distance between each bomb

    [Header("Drop Settings")]
    [SerializeField] private float dropHeight = 20f;

    [Header("Timing Settings")]
    [SerializeField] private float offDuration = 2f;     // Time before bombs drop
    [SerializeField] private float dropInterval = 4f;    // Time after drop before repeating

    [Header("References")]
    [SerializeField] private BombPool bombPool;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(offDuration);

            DropBombRow(new Vector3(
                transform.position.x,
                0f,
                transform.position.z
            ));

            yield return new WaitForSeconds(dropInterval);
        }
    }

    private void DropBombRow(Vector3 center)
    {
        float totalLength = (bombCount - 1) * bombSpacing;
        float startZ = center.z - totalLength / 2f;
        float y = dropHeight;
        float x = center.x;

        for (int i = 0; i < bombCount; i++)
        {
            Vector3 pos = new Vector3(
                x,
                y,
                startZ + i * bombSpacing   // << vertical spacing
            );

            bombPool.GetBomb(pos, Quaternion.identity);
        }
    }
}
