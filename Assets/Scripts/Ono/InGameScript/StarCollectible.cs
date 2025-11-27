using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarCollectible : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the LevelStarCounter in this level.")]
    public LevelStarCounter starCounter;

    [Header("Settings")]
    [Tooltip("Optional: only the player with this tag can collect.")]
    public string playerTag = "Player";

    [Header("FX (optional)")]
    public GameObject collectVfx;
    public AudioClip collectSfx;
    public float destroyDelay = 0.0f;

    private bool _collected;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        _collected = true;

        if (starCounter != null)
        {
            starCounter.AddStar();
        }
        else
        {
            Debug.LogWarning("[StarCollectible] No LevelStarCounter assigned.", this);
        }

        if (collectVfx != null)
        {
            Instantiate(collectVfx, transform.position, Quaternion.identity);
        }

        if (collectSfx != null)
        {
            AudioSource.PlayClipAtPoint(collectSfx, transform.position);
        }

        if (destroyDelay <= 0f)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject, destroyDelay);
        }
    }
}
