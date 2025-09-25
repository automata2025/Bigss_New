using UnityEngine;

[RequireComponent(typeof(P_Control_Physics))]
public class CrabChainDetacher : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode detachKey = KeyCode.Space;

    [Header("Shot Tuning")]
    [SerializeField] private float velocityScale = 1.0f;     // 1.0 = use history velocity magnitude as-is
    [SerializeField] private float minSpeed = 4f;            // floor so it always pops
    [SerializeField] private float maxSpeed = 40f;           // clamp for stability
    [SerializeField] private bool inheritUpward = false;     // add a tiny upward kick if you like
    [SerializeField] private float upwardKick = 1.5f;

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

        // The segment we detached was index (count before removal - 1).
        // We want its delay on the path: crabTimeGap * (index+1).
        // Since we've *just* removed it, its index was equal to current count (after removal) + 0.
        // Easiest is to compute using delay of "new last + 1". But we need the old last’s delay.
        // So compute as if it still existed: delay = crabTimeGap * (currentCount + 1).
        int currentCount = GetCurrentFollowerCount(); // after removal
        float delay = _ctrl.CrabTimeGap * (currentCount + 1);

        // Sample two nearby points to produce a history-based velocity vector.
        // Use the SAME sampling logic as the followers (no formula changes).
        Vector3 p1 = _ctrl.SamplePointAtDelay(delay);

        // Step a small delta further back in time to get a direction. We use the exact segment duration:
        // If we're between head and the most recent record, the "segment duration" is HeadToRecord.
        // Otherwise it is recordInterval. This mirrors your original piecewise interpolation.
        float segmentDt = (delay <= _ctrl.HeadToRecord) ? _ctrl.HeadToRecord : _ctrl.RecordInterval;
        // use half-step for a cleaner derivative while avoiding overshoot at boundaries
        float h = Mathf.Max(0.0001f, segmentDt * 0.5f);
        Vector3 p0 = _ctrl.SamplePointAtDelay(delay + h);

        Vector3 vHist = (p1 - p0);
        float speed = vHist.magnitude / h;

        // Build initial direction & speed
        Vector3 dir = vHist.sqrMagnitude > 1e-8f ? vHist.normalized : (seg.forward);
        float shotSpeed = Mathf.Clamp(speed * velocityScale, minSpeed, maxSpeed);

        Vector3 launchVel = dir * shotSpeed;
        if (inheritUpward) launchVel += Vector3.up * upwardKick;

        // Place & orient the detached segment cleanly at its sampled position
        seg.position = p1;
        if (dir.sqrMagnitude > 0.000001f)
            seg.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // Kick it with physics if it has a Rigidbody, otherwise translate in an updater on the segment
        if (seg.TryGetComponent<Rigidbody>(out var segRb))
        {
            segRb.isKinematic = false; // make sure it’s free
            segRb.velocity = launchVel;
        }
        else
        {
            // No Rigidbody? Move it manually later in its own behaviour.
            // Example: you could add a light-weight projectile script that uses 'launchVel'.
        }

        return true;
    }

    private int GetCurrentFollowerCount()
    {
        // We can’t see the private crabPart list directly (good encapsulation).
        // A simple approach: count scene children tagged/layered as parts, or expose a tiny read-only count on P_Control_Physics.
        // If you prefer the latter, add: public int FollowerCount => crabPart.Count; to P_Control_Physics and use it here.
        // For now, we assume parts are siblings spawned by the controller and carry a specific tag:
        const string partTag = "CrabPart";
        return GameObject.FindGameObjectsWithTag(partTag).Length; // OK for now; swap to a property for perf.
    }
}
