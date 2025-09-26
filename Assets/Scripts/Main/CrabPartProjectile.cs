using System;
using System.Collections.Generic;
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

    // Small cache to avoid GC when notifying handlers
    private static readonly List<ICrabImpactHandler> _handlerCache = new List<ICrabImpactHandler>(8);

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
            return;
        }

        _active = true;
        _life = 0f;
        _bounceCount = 0;

        _rb.isKinematic = false;
        _rb.velocity = initialVelocity;
        _rb.angularVelocity = Vector3.zero;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        if (!_active) return;

        _life += Time.deltaTime;
        if (_life >= _maxLife)
        {
            Explode(transform.position, -transform.forward, null);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_active) return;

        int otherLayer = collision.collider.gameObject.layer;
        bool isExplodeLayer = ((_explodeMask.value & (1 << otherLayer)) != 0);

        // Contact data for direct-hit notify
        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 hitNormal = collision.GetContact(0).normal;

        if (isExplodeLayer)
        {
            Explode(hitPoint, hitNormal, collision.collider);
            return;
        }

        _bounceCount++;
        if (_bounceCount > _maxBounces)
        {
            Explode(hitPoint, hitNormal, collision.collider);
        }
    }

    private void Explode(Vector3 point, Vector3 normal, Collider directHit)
    {
        if (!_active) return;
        _active = false;

        Vector3 incomingVel = _rb != null ? _rb.velocity : Vector3.zero;
        var ctx = new CrabImpactContext(point,normal,incomingVel,energy: 1f, radius: 0f,instigator: gameObject,projectile: gameObject);

        NotifyHandlersOnCollider(directHit, ctx);

        if (_rb != null) _rb.isKinematic = true;
        Destroy(gameObject);
    }

    private void Explode()
    {
        Explode(transform.position, -transform.forward, null);
    }

    private void NotifyHandlersOnCollider(Collider col, CrabImpactContext ctx)
    {
        if (col == null) return;

        col.GetComponentsInChildren(true, _handlerCache);
        for (int i = 0; i < _handlerCache.Count; i++)
        {
            bool consumed = _handlerCache[i].OnCrabImpact(ctx);
            if (consumed) break; 
        }
        _handlerCache.Clear();
    }
}
