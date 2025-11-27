using UnityEngine;

public class WallTrigger : MonoBehaviour
{
    [Header("Wall References")]
    [SerializeField] private MovingWall movingWall;            // Existing wall in scene
    [SerializeField] private MovingWall movingWallPrefab;      // Prefab option

    [Header("Spawn Settings (Optional)")]
    [SerializeField] private Transform spawnPoint;             // Where prefab should appear

    private void Start()
    {
        // If no scene object, but prefab exists -> Instantiate
        if (movingWall == null && movingWallPrefab != null)
        {
            Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

            movingWall = Instantiate(movingWallPrefab, pos, rot);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && movingWall != null)
        {
            movingWall.Activate();
        }
    }
}
