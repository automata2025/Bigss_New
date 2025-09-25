using UnityEngine;

public class CrabPartProjectile : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _col;

    private LayerMask _explodeMask;
    private int _maxBounces;
    private int _bounceCount;
    private float _maxLife;
    private float _life;

    private bool _active;

    public void Configure(Rigidbody rb, Collider col, LayerMask explodeMask, int maxBounces, float maxLife)
    {
        _rb = rb;
        _col = col;
        _explodeMask = explodeMask;
        _maxBounces = Mathf.Max(0, maxBounces);
        _maxLife = Mathf.Max(0.1f, maxLife);
    }

    public void Activate(Vector3 initialVelocity)
    {
        if (_rb == null || _col == null)
        {
            Debug.LogWarning("CrabPartProjectile missing RB/Collider.");
            return;
        }

        _active = true;
        _life = 0f;
        _bounceCount = 0;

        _rb.isKinematic = false;
        _rb.velocity = initialVelocity;
        _rb.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!_active) return;

        _life += Time.deltaTime;
        if (_life >= _maxLife)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_active) return;

        int otherLayer = collision.collider.gameObject.layer;
        bool isExplodeLayer = ((_explodeMask.value & (1 << otherLayer)) != 0);

        if (isExplodeLayer)
        {
            Explode(); // explode immediately on walls/lasers/etc.
            return;
        }

        _bounceCount++;
        if (_bounceCount > _maxBounces)
        {
            Explode();
        }
        // else let physics material handle the bounce automatically
    }

    private void Explode()
    {
        if (!_active) return;
        _active = false;

        // TODO: spawn VFX/SFX here

        // disable physics and destroy
        if (_rb != null) _rb.isKinematic = true;
        Destroy(gameObject);
    }
}
