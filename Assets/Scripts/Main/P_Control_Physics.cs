using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class P_Control_Physics : MonoBehaviour
{
    // === Player Movement (same names as your original) ===
    [SerializeField] private int crabAmounts = 4;
    [SerializeField] private float moveSpeed = 50f;

    [SerializeField] private float steerAngle = 20f;   // degrees per (unit speed) per second
    [SerializeField] private float Traction = 1f;      // align velocity to forward per second

    // === Followers (same as original) ===
    [SerializeField] private float miniCrabSpeed = 5f;
    [SerializeField] private float crabTimeGap = 0.3f;
    [SerializeField] private GameObject crabPrefab;

    // === Recording (same as original) ===
    [SerializeField] private List<Vector3> positionsHistory = new List<Vector3>();
    [SerializeField] private float recordInterval = 0.1f;

    private readonly List<GameObject> crabPart = new List<GameObject>();

    private float lastRecordTime;      // last time we stored a sample (wall clock to match original math)
    private float recordAccumulator;   // accumulates fixedDeltaTime to hit recordInterval

    // Your "velocity" as before
    private Vector3 moveForce;

    // Input cached in Update, applied in FixedUpdate
    private float vInput, hInput;

    private Rigidbody rb;

    // Sweep (anti-tunneling) skin offset
    private const float skin = 0.01f;

    // --- Read-only accessors for other systems (detach, hazards, etc.) ---
    public Vector3 HeadPosition => rb != null ? rb.position : transform.position;
    public Vector3 PlanarVelocity => moveForce;
    public float HeadToRecord => Mathf.Max(0.000001f, Time.time - lastRecordTime);
    public float RecordInterval => recordInterval;
    public float CrabTimeGap => crabTimeGap;
    public int FollowerCount => crabPart.Count;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Physics setup for smooth motion with collisions and gravity
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;                // smooth rendering between physics steps
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // best with MovePosition movers
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        // Keep rb.drag = 0 so your Traction math controls the feel
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void Start()
    {
        // Spawn followers (spawn at head position so they don't appear at world origin)
        for (int i = 0; i < crabAmounts; i++)
            SpawnCrab();

        // Initialize recorder so we don’t get a zero headToRecord on first frame
        positionsHistory.Clear();
        positionsHistory.Insert(0, rb.position);
        lastRecordTime = Time.time;
        recordAccumulator = 0f;
    }

    void Update()
    {
        // Cache input each render frame; consumed in FixedUpdate
        vInput = Input.GetAxis("Vertical");
        hInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // === Moving (same formula as original, using cached input) ===
        moveForce += transform.forward * moveSpeed * vInput * dt;

        // === Steering (same idea as original) ===
        float steerInput = hInput;
        float turnAngle = moveForce.magnitude * steerAngle * dt;
        Quaternion nextRotation = rb.rotation * Quaternion.AngleAxis(turnAngle * steerInput, Vector3.up);
        rb.MoveRotation(nextRotation);

        // === Traction (same Lerp normal * magnitude as original) ===
        Vector3 forwardAfterTurn = nextRotation * Vector3.forward;
        if (moveForce.sqrMagnitude > 0.0000001f)
        {
            moveForce = Vector3.Lerp(moveForce.normalized, forwardAfterTurn, Traction * dt) * moveForce.magnitude;
        }

        // === Apply movement via physics WITH SWEEP to prevent tunneling ===
        Vector3 displacement = moveForce * dt;
        float dist = displacement.magnitude;

        if (dist > 0f)
        {
            Vector3 dir = displacement / dist;

            if (rb.SweepTest(dir, out RaycastHit hit, dist + skin, QueryTriggerInteraction.Ignore))
            {
                // Move up to the impact point minus skin
                float allowed = Mathf.Max(0f, hit.distance - skin);
                rb.MovePosition(rb.position + dir * allowed);

                // Slide along the surface by removing the into-wall component of your velocity
                moveForce = Vector3.ProjectOnPlane(moveForce, hit.normal);

                float into = Vector3.Dot(moveForce, hit.normal);
                if (into < 0f) moveForce -= hit.normal * into;
            }
            else
            {
                rb.MovePosition(rb.position + displacement);
            }
        }
        else
        {
            rb.MovePosition(rb.position);
        }

        // Debug (optional)
        if (moveForce.sqrMagnitude > 0.000001f)
            Debug.DrawRay(rb.position, moveForce.normalized * 3f);
        Debug.DrawRay(rb.position, forwardAfterTurn * 3f, Color.blue);

        // === RECORD HISTORY IN PHYSICS TIME ===
        // Accumulate fixed time and write samples at recordInterval cadence
        recordAccumulator += dt;
        while (recordAccumulator >= recordInterval)
        {
            positionsHistory.Insert(0, rb.position); // newest at index 0 (same as your original)

            int maxHistoryCount = Mathf.CeilToInt((crabAmounts + 1) * crabTimeGap / recordInterval);
            while (positionsHistory.Count > maxHistoryCount && maxHistoryCount >= 0)
                positionsHistory.RemoveAt(positionsHistory.Count - 1);

            lastRecordTime = Time.time; // wall clock is fine for your math; we just ensure cadence matches physics
            recordAccumulator -= recordInterval;
        }

        // === FOLLOWERS IN PHYSICS TIME (removes jitter from Update/physics mismatch) ===
        int bodyPartIndex = 0;

        foreach (var body in crabPart)
        {
            float delayFromHead = crabTimeGap * (bodyPartIndex + 1);
            float headToRecord = Mathf.Max(0.000001f, Time.time - lastRecordTime); // guard zero

            int startindex = -1;
            float remaining = delayFromHead;

            if (remaining >= headToRecord)
            {
                startindex++;
                remaining -= headToRecord;
            }
            while (remaining >= recordInterval)
            {
                startindex++;
                remaining -= recordInterval;
            }

            Vector3 point = rb.position;

            if (positionsHistory.Count > 0)
            {
                if (startindex == -1)
                {
                    // Between current head and most recent recorded sample
                    point = Vector3.Lerp(rb.position, positionsHistory[0], remaining / headToRecord);
                }
                else
                {
                    int indexA = Mathf.Min(startindex, positionsHistory.Count - 1);
                    int indexB = Mathf.Min(startindex + 1, positionsHistory.Count - 1);
                    point = Vector3.Lerp(positionsHistory[indexA], positionsHistory[indexB], remaining / recordInterval);
                }
            }

            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * miniCrabSpeed * dt; // use fixed dt for consistency

            if (moveDirection.sqrMagnitude > 0.00001f)
                body.transform.rotation = Quaternion.LookRotation(moveDirection);

            bodyPartIndex++;
        }
    }

    // === Sampling helper for detach: get a point on the history at an exact delay ===
    public Vector3 SamplePointAtDelay(float delay)
    {
        float headToRecord = HeadToRecord;
        int startindex = -1;
        float remaining = delay;

        if (remaining >= headToRecord)
        {
            startindex++;
            remaining -= headToRecord;
        }
        while (remaining >= recordInterval)
        {
            startindex++;
            remaining -= recordInterval;
        }

        Vector3 point = rb.position;

        if (positionsHistory.Count > 0)
        {
            if (startindex == -1)
            {
                // Between current head and most recent recorded sample
                point = Vector3.Lerp(rb.position, positionsHistory[0], remaining / Mathf.Max(headToRecord, 0.000001f));
            }
            else
            {
                int indexA = Mathf.Min(startindex, positionsHistory.Count - 1);
                int indexB = Mathf.Min(startindex + 1, positionsHistory.Count - 1);
                point = Vector3.Lerp(positionsHistory[indexA], positionsHistory[indexB], remaining / recordInterval);
            }
        }

        return point;
    }

    // === Detach helper: remove last follower and return its Transform ===
    public bool TryDetachLast(out Transform detached)
    {
        detached = null;
        int count = crabPart.Count;
        if (count == 0) return false;

        int last = count - 1;
        GameObject obj = crabPart[last];
        crabPart.RemoveAt(last);
        detached = obj != null ? obj.transform : null;
        return detached != null;
    }

    private void SpawnCrab()
    {
        // Spawn at head position/rotation so parts begin aligned (prevents popping from world origin)
        GameObject crab = Instantiate(crabPrefab, rb.position, rb.rotation);
        crabPart.Add(crab);
    }
}
