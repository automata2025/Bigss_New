using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class P_Control_Physics : MonoBehaviour
{
    [SerializeField] private int crabAmounts = 4;
    [SerializeField] private float moveSpeed = 50f;

    [SerializeField] private float steerAngle = 20f;   
    [SerializeField] private float Traction = 1f;      

    [SerializeField] private float miniCrabSpeed = 5f;
    [SerializeField] private float crabTimeGap = 0.3f;
    [SerializeField] private GameObject crabPrefab;

    [SerializeField] private List<Vector3> positionsHistory = new List<Vector3>();
    [SerializeField] private float recordInterval = 0.1f;

    private readonly List<GameObject> crabPart = new List<GameObject>();

    private float lastRecordTime;     
    private float recordAccumulator;

    private Vector3 moveForce;

    private float vInput, hInput;

    private Rigidbody rb;

    // Sweep (anti-tunneling) skin offset
    private const float skin = 0.01f;

    public Vector3 HeadPosition => rb != null ? rb.position : transform.position;
    public Vector3 PlanarVelocity => moveForce;
    public float HeadToRecord => Mathf.Max(0.000001f, Time.time - lastRecordTime);
    public float RecordInterval => recordInterval;
    public float CrabTimeGap => crabTimeGap;
    public int FollowerCount => crabPart.Count;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        //for smooth motion with collisions and gravity
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;                
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void Start()
    {
        for (int i = 0; i < crabAmounts; i++)
            SpawnCrab();

        positionsHistory.Clear();
        positionsHistory.Insert(0, rb.position);
        lastRecordTime = Time.time;
        recordAccumulator = 0f;
    }

    void Update()
    {
        vInput = Input.GetAxis("Vertical");
        hInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        moveForce += transform.forward * moveSpeed * vInput * dt;

        float steerInput = hInput;
        float turnAngle = moveForce.magnitude * steerAngle * dt;
        Quaternion nextRotation = rb.rotation * Quaternion.AngleAxis(turnAngle * steerInput, Vector3.up);
        rb.MoveRotation(nextRotation);

        Vector3 forwardAfterTurn = nextRotation * Vector3.forward;
        if (moveForce.sqrMagnitude > 0.0000001f)
        {
            moveForce = Vector3.Lerp(moveForce.normalized, forwardAfterTurn, Traction * dt) * moveForce.magnitude;
        }

        Vector3 displacement = moveForce * dt;
        float dist = displacement.magnitude;

        if (dist > 0f)
        {
            Vector3 dir = displacement / dist;

            if (rb.SweepTest(dir, out RaycastHit hit, dist + skin, QueryTriggerInteraction.Ignore))
            {
                float allowed = Mathf.Max(0f, hit.distance - skin);
                rb.MovePosition(rb.position + dir * allowed);

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

        // Debug
        if (moveForce.sqrMagnitude > 0.000001f)
            Debug.DrawRay(rb.position, moveForce.normalized * 3f);
        Debug.DrawRay(rb.position, forwardAfterTurn * 3f, Color.blue);

        recordAccumulator += dt;
        while (recordAccumulator >= recordInterval)
        {
            positionsHistory.Insert(0, rb.position); 

            int maxHistoryCount = Mathf.CeilToInt((crabAmounts + 1) * crabTimeGap / recordInterval);
            while (positionsHistory.Count > maxHistoryCount && maxHistoryCount >= 0)
                positionsHistory.RemoveAt(positionsHistory.Count - 1);

            lastRecordTime = Time.time; 
            recordAccumulator -= recordInterval;
        }

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
            body.transform.position += moveDirection * miniCrabSpeed * dt; 

            if (moveDirection.sqrMagnitude > 0.00001f)
                body.transform.rotation = Quaternion.LookRotation(moveDirection);

            bodyPartIndex++;
        }
    }

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
        GameObject crab = Instantiate(crabPrefab, rb.position, rb.rotation);
        crabPart.Add(crab);
    }
}
