using UnityEngine;

[RequireComponent(typeof(P_Control_Physics))]
public class CrabChainDetacher : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode detachKey = KeyCode.Space;

    [Header("Launch")]
    [SerializeField] private float velocityScale = 1.0f; // 1.0 uses history speed; >1 boosts
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private bool addUpwardKick = false;
    [SerializeField] private float upwardKick = 1.5f;

    [Header("Physics Setup (for the detached part)")]
    [SerializeField] private float rbMass = 1f;
    [SerializeField] private float rbDrag = 0f;
    [SerializeField] private float rbAngularDrag = 0.05f;
    [SerializeField] private PhysicMaterial bounceMaterial; // optional; assign a bouncy material in Inspector
    [SerializeField] private Vector3 colliderCenter = new Vector3(0, 0.2f, 0);
    [SerializeField] private float colliderRadius = 0.2f;
    [SerializeField] private LayerMask explodeOnLayers;   // walls/lasers etc.
    [SerializeField] private string detachedLayerName = "DetachedCrab"; // optional layer

    [Header("Lifetime")]
    [SerializeField] private int maxBounces = 2;      // explode after this many bounces if not already
    [SerializeField] private float maxLifetime = 6f;  // safety despawn

    private P_Control_Physics _ctrl;

    private void Awake()
    {
        _ctrl = GetComponent<P_Control_Physics>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(detachKey))
            DetachLastSegment();
    }

    public bool DetachLastSegment()
    {
        if (!_ctrl.TryDetachLast(out Transform seg)) return false;

        // Compute the delay for the segment we just removed:
        // old index = (new follower count) + 1 -> delay = gap * (index)
        int currentCount = _ctrl.FollowerCount; // after removal
        float delay = _ctrl.CrabTimeGap * (currentCount + 1);

        // Sample position at delay (your exact algorithm via helper)
        Vector3 p1 = _ctrl.SamplePointAtDelay(delay);

        // Derive direction/speed from a nearby earlier sample (finite difference)
        float segmentDt = (delay <= _ctrl.HeadToRecord) ? _ctrl.HeadToRecord : _ctrl.RecordInterval;
        float h = Mathf.Max(0.0001f, segmentDt * 0.5f);
        Vector3 p0 = _ctrl.SamplePointAtDelay(delay + h);

        Vector3 vHist = (p1 - p0);
        float speed = vHist.magnitude / h;
        Vector3 dir = vHist.sqrMagnitude > 1e-8f ? vHist.normalized : seg.forward;

        float launchSpeed = Mathf.Clamp(speed * velocityScale, minSpeed, maxSpeed);
        Vector3 launchVel = dir * launchSpeed;
        if (addUpwardKick) launchVel += Vector3.up * upwardKick;

        // Place the segment at the sampled position & facing
        seg.position = p1;
        seg.rotation = Quaternion.LookRotation(dir, Vector3.up);
        seg.SetParent(null);

        // Ensure physics components exist & are enabled
        Rigidbody segRb = EnsureRigidbody(seg.gameObject);
        Collider segCol = EnsureCollider(seg.gameObject);

        // Optional: move to a dedicated layer
        if (!string.IsNullOrEmpty(detachedLayerName))
        {
            int layerIdx = LayerMask.NameToLayer(detachedLayerName);
            if (layerIdx >= 0) seg.gameObject.layer = layerIdx;
        }

        // Hand off to projectile behavior
        var proj = seg.GetComponent<CrabPartProjectile>();
        if (proj == null) proj = seg.gameObject.AddComponent<CrabPartProjectile>();
        proj.Configure(segRb, segCol, explodeOnLayers, maxBounces, maxLifetime);
        proj.Activate(launchVel);

        return true;
    }

    private Rigidbody EnsureRigidbody(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();

        rb.mass = rbMass;
        rb.drag = rbDrag;
        rb.angularDrag = rbAngularDrag;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;

        return rb;
    }

    private Collider EnsureCollider(GameObject go)
    {
        Collider col = go.GetComponent<Collider>();

        if (col == null)
        {
            var sphere = go.AddComponent<SphereCollider>();
            sphere.center = colliderCenter;
            sphere.radius = colliderRadius;
            col = sphere;
        }

        col.isTrigger = false;
        if (bounceMaterial != null) col.material = bounceMaterial;

        return col;
    }
}
