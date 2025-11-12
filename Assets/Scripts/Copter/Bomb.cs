using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public LayerMask affectedLayers;
    public GameObject explosionEffectPrefab;
    public float deactivateDelay = 0.2f;

    private Rigidbody rb;
    private BombPool pool;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // Reset physics state
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;
        rb.isKinematic = false;

        // Re-enable visuals/collisions
        var rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = true;
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    public void AssignPool(BombPool bombPool)
    {
        pool = bombPool;
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        // Visual explosion effect
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Apply physics force to nearby rigidbodies
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);
        foreach (Collider col in colliders)
        {
            Rigidbody r = col.attachedRigidbody;
            if (r != null)
                r.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
        }

        // Disable this bomb visually
        var rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;
        var c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        rb.isKinematic = true;

        // Return to pool after short delay
        Invoke(nameof(ReturnToPool), deactivateDelay);
    }

    void ReturnToPool()
    {
        if (pool != null)
            pool.ReturnBomb(gameObject);
        else
            gameObject.SetActive(false);
    }
}
