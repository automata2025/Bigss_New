using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class P_MovementPhysics : MonoBehaviour
{
    // ----- Tuning (private + serialized) -----
    [Header("Speed & Steering")]
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private float acceleration = 40f;      // forward push
    [SerializeField] private float reverseAcceleration = 25f;
    [SerializeField] private float steerPower = 6f;         // how fast we rotate
    [SerializeField] private float steerLerp = 12f;         // smoothing

    [Header("Drift & Traction")]
    [SerializeField] private float driftFactor = 0.92f;     // lower = more sideways slide
    [SerializeField] private float traction = 6f;           // how quickly we align with velocity
    [SerializeField] private float lateralFriction = 6f;    // kills sideways velocity over time

    [Header("Damping")]
    [SerializeField] private float linearDrag = 0.2f;
    [SerializeField] private float angularDrag = 1.5f;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckRadius = 0.35f;
    [SerializeField] private float groundCheckOffset = 0.1f;

    [Header("Input")]
    [SerializeField] private bool cameraRelative = true;    // if true, WASD are relative to camera facing
    [SerializeField] private Transform cameraRef;           // assign your main camera (or leave null to use Camera.main)

    [Header("Collision")]
    [SerializeField] private float continuousSpeedThreshold = 10f; // switch to continuous at high speed to prevent tunneling

    // ----- State (public read-only) -----
    public float CurrentSpeed => _rb.velocity.magnitude;
    public bool IsGrounded { get; private set; }
    public Vector3 PlanarVelocity { get; private set; }

    // ----- Internals -----
    private Rigidbody _rb;
    private Vector2 _moveInput;    // x = left/right, y = forward/back
    private float _steerInput;     // for gamepad triggers/keys if you want separate steer (optional)

    // Cached for camera-relative
    private Transform _camTr;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // we’ll upgrade dynamically if needed
        _rb.drag = linearDrag;
        _rb.angularDrag = angularDrag;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // keep upright; we rotate only around Y

        _camTr = cameraRef != null ? cameraRef : (Camera.main != null ? Camera.main.transform : null);
    }

    private void Update()
    {
        // --- Input (simple legacy axes; swap to new Input System later if you want) ---
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        _moveInput = new Vector2(h, v).normalized;

        // Optional: separate steer input (e.g., left/right arrows) — not required with cameraRelative steering
        // _steerInput = Input.GetAxisRaw("Horizontal");

        // Dynamic collision mode upgrade to prevent tunneling when very fast
        _rb.collisionDetectionMode = (CurrentSpeed > continuousSpeedThreshold)
            ? CollisionDetectionMode.ContinuousDynamic
            : CollisionDetectionMode.Discrete;
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        UpdatePlanarVelocity();

        // Compute desired move direction (world space)
        Vector3 desiredDir = ComputeDesiredDirection(_moveInput);

        // Acceleration & speed clamp
        ApplyPlanarAcceleration(desiredDir);

        // Steering toward velocity (car-like yaw)
        ApplySteering(desiredDir);

        // Drift/traction shaping
        ApplyDriftModel();

        // Lateral friction to kill side slip gradually
        ApplyLateralFriction();
    }

    // ---------- Helpers ----------

    private void UpdateGrounded()
    {
        Vector3 c = transform.position + Vector3.down * groundCheckOffset;
        IsGrounded = Physics.CheckSphere(c, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdatePlanarVelocity()
    {
        Vector3 v = _rb.velocity;
        PlanarVelocity = new Vector3(v.x, 0f, v.z);
    }

    private Vector3 ComputeDesiredDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        if (cameraRelative && _camTr != null)
        {
            Vector3 camF = _camTr.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = _camTr.right; camR.y = 0f; camR.Normalize();
            Vector3 world = camF * input.y + camR * input.x;
            return world.normalized;
        }
        else
        {
            // World-relative: forward = +Z, right = +X
            return new Vector3(input.x, 0f, input.y).normalized;
        }
    }

    private void ApplyPlanarAcceleration(Vector3 desiredDir)
    {
        if (!IsGrounded) return;

        float curSpeed = PlanarVelocity.magnitude;
        if (desiredDir == Vector3.zero)
        {
            // No input: natural drag handles slow-down; nothing to add
            return;
        }

        // Forward vs reverse accel
        float dotForward = Vector3.Dot(desiredDir, transform.forward);
        float accel = dotForward >= 0f ? acceleration : reverseAcceleration;

        // Project accel along desired direction (planar)
        Vector3 force = desiredDir * accel;

        // Don’t exceed max speed much — allow a tiny buffer for turning/sliding feel
        if (curSpeed > maxSpeed * 1.05f && Vector3.Dot(PlanarVelocity, desiredDir) > 0f)
            return;

        _rb.AddForce(force, ForceMode.Acceleration);
    }

    private void ApplySteering(Vector3 desiredDir)
    {
        // Steering is strongest when moving; weak when almost stopped
        Vector3 velocity = PlanarVelocity;
        float speed01 = Mathf.Clamp01(velocity.magnitude / Mathf.Max(1f, maxSpeed));

        // Desired facing: if there is input, prefer input direction; otherwise align to velocity
        Vector3 lookDir = desiredDir != Vector3.zero ? desiredDir : (velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward);

        // Smoothly rotate toward lookDir around Y only
        Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
        float steerStrength = steerPower * (0.35f + 0.65f * speed01); // scale with speed (never 0)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, steerLerp * steerStrength * Time.fixedDeltaTime);
    }

    private void ApplyDriftModel()
    {
        // Blend current velocity toward our forward direction to simulate traction
        if (PlanarVelocity.sqrMagnitude < 0.0001f) return;

        Vector3 forward = transform.forward;
        Vector3 v = PlanarVelocity;

        // Split velocity into forward & lateral components
        Vector3 fwdComponent = Vector3.Project(v, forward);
        Vector3 latComponent = v - fwdComponent;

        // Drift: retain some lateral (driftFactor) and pull toward forward (traction)
        latComponent *= driftFactor; // retains side slip
        Vector3 targetPlanar = fwdComponent + latComponent;

        // Traction pulls us to targetPlanar
        Vector3 newPlanar = Vector3.Lerp(v, targetPlanar, Mathf.Clamp01(traction * Time.fixedDeltaTime));

        // Keep Y from gravity
        _rb.velocity = new Vector3(newPlanar.x, _rb.velocity.y, newPlanar.z);
    }

    private void ApplyLateralFriction()
    {
        if (PlanarVelocity.sqrMagnitude < 0.0001f) return;

        Vector3 right = transform.right;
        Vector3 v = PlanarVelocity;
        Vector3 lateral = Vector3.Project(v, right);

        // remove a portion of lateral each step (acts like sideways tire friction)
        Vector3 newPlanar = v - lateral * Mathf.Clamp01(lateralFriction * Time.fixedDeltaTime);
        _rb.velocity = new Vector3(newPlanar.x, _rb.velocity.y, newPlanar.z);
    }

#if UNITY_EDITOR
    // Optional gizmos for grounding debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 c = transform.position + Vector3.down * groundCheckOffset;
        Gizmos.DrawWireSphere(c, groundCheckRadius);
    }
#endif
}

