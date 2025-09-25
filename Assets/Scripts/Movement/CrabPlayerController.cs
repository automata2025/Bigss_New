using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class CrabPlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int crabAmount = 4;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float steerAngle = 20f;   // degrees per (unit speed) per second
    [SerializeField] private float traction = 1f;      // align velocity to forward per second

    [Header("Followers")]
    [SerializeField] private float miniCrabSpeed = 5f;
    [SerializeField] private float crabTimeGap = 0.3f;
    [SerializeField] private GameObject crabPrefab;

    [Header("Recording")]
    [SerializeField] private float recordInterval = 0.1f;

    [Header("Handbrake")]
    [SerializeField] private KeyCode handbrakeKey = KeyCode.LeftShift;
    [SerializeField] private float handbrakeForwardDecel = 60f;
    [SerializeField] private float handbrakeLateralBoost = 3f;
    [SerializeField, Range(0.05f, 1f)] private float handbrakeTractionScale = 0.5f;

    [Header("Optional Tweaks")]
    [SerializeField] private float maxPlanarSpeed = 30f;
    [SerializeField] private float baseLateralFriction = 0f;

    // --- Components & modules ---
    private Rigidbody _rb;
    private PhysicsMover _mover;
    private PositionHistoryBuffer _history;
    private FollowerChainController _followers;

    // --- Input cache ---
    private float _vInput;
    private float _hInput;
    private bool _handbrakeHeld;

    // Properties (read-only access if other systems need them later)
    public Vector3 PlanarVelocity => _mover.PlanarVelocity;
    public float CurrentSpeed => _mover.CurrentSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.drag = 0f;
        _rb.angularDrag = 0f;

        // Modules
        _mover = new PhysicsMover(
            moveSpeed,
            steerAngle,
            traction,
            handbrakeForwardDecel,
            handbrakeLateralBoost,
            handbrakeTractionScale,
            maxPlanarSpeed,
            baseLateralFriction
        );

        _history = new PositionHistoryBuffer();

        _followers = new FollowerChainController(
            crabPrefab,
            crabAmount,
            miniCrabSpeed,
            crabTimeGap
        );
    }

    private void Start()
    {
        _followers.SpawnInitial(crabAmount, transform.position, transform.rotation);
        // Capacity: enough to cover all followers’ delay span
        int maxSamples = Mathf.CeilToInt((_followers.RequiredHistorySeconds + recordInterval) / recordInterval) + 2;
        _history.Initialize(maxSamples, transform.position);
    }

    private void Update()
    {
        _vInput = Input.GetAxis("Vertical");
        _hInput = Input.GetAxis("Horizontal");
        _handbrakeHeld = Input.GetKey(handbrakeKey);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Move/rotate with drift & sweep slide
        _mover.Step(_rb, _vInput, _hInput, _handbrakeHeld, dt);

        // Record at a stable cadence in physics time
        _history.AccumulateAndRecord(recordInterval, dt, _rb.position);

        // Update followers with the same timing math you used
        _followers.UpdateFollowers(in _history, _rb.position, _mover.ForwardAfterTurn, dt);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_mover != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 p = _rb != null ? _rb.position : transform.position;
            Gizmos.DrawLine(p, p + _mover.ForwardAfterTurn * 3f);
        }
    }
#endif
}

