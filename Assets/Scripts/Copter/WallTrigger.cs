using UnityEngine;

public class WallTrigger : MonoBehaviour
{
    [SerializeField] private MovingWall movingWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            movingWall.Activate();
        }
    }
}
