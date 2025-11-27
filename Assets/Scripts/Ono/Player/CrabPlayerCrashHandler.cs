using System.Collections;
using UnityEngine;

[RequireComponent(typeof(P_Control_Physics))]
[RequireComponent(typeof(Rigidbody))]
public class CrabPlayerCrashHandler : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 0.25f;

    [Header("Hazard Detection")]
    [SerializeField] private LayerMask hazardLayers;
    [SerializeField] private bool useTagCheck = true;
    [SerializeField] private string hazardTag = "Hazard";

    [Header("UI")]
    [SerializeField] private GameOverUI gameOverUI;   

    private P_Control_Physics _ctrl;
    private Rigidbody _rb;
    private bool _isRespawning;

    private void Awake()
    {
        _ctrl = GetComponent<P_Control_Physics>();
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsHazard(collision.gameObject))
        {
            HandleHit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHazard(other.gameObject))
        {
            HandleHit();
        }
    }

    private bool IsHazard(GameObject go)
    {
        bool layerMatch = hazardLayers != 0 &&
                          ((hazardLayers.value & (1 << go.layer)) != 0);

        bool tagMatch = useTagCheck &&
                        !string.IsNullOrEmpty(hazardTag) &&
                        go.CompareTag(hazardTag);

        return layerMatch || tagMatch;
    }

    private void HandleHit()
    {
        if (_isRespawning)
            return;

        int crabsLeft = _ctrl.FollowerCount;
        Debug.Log($"[CrashHandler] Hit hazard, crabsLeft = {crabsLeft}");

        if (crabsLeft > 0)
        {
            // Lose 1 crab and respawn
            if (_ctrl.TryDetachLast(out Transform seg) && seg != null)
            {
                Destroy(seg.gameObject);
            }

            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // No crabs left: Game Over
            Debug.Log("[CrashHandler] GAME OVER branch");

            if (gameOverUI != null)
            {
                gameOverUI.ShowGameOver();
            }
            else
            {
                Debug.LogWarning("[CrashHandler] GameOverUI reference not set!");
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        _isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        if (respawnPoint != null)
        {
            _rb.position = respawnPoint.position;
            _rb.rotation = respawnPoint.rotation;
        }

        _isRespawning = false;
    }
}
