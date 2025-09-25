using UnityEngine;

/// <summary>
/// Handles player planar motion: acceleration, steering, traction, handbrake, and swept collision slide.
/// </summary>
public sealed class PhysicsMover
{
    // Tunables
    private readonly float _moveSpeed;
    private readonly float _steerAngle;
    private readonly float _traction;
    private readonly float _handbrakeForwardDecel;
    private readonly float _handbrakeLateralBoost;
    private readonly float _handbrakeTractionScale;
    private readonly float _maxPlanarSpeed;
    private readonly float _baseLateralFriction;

    // State
    private Vector3 _moveForce;
    private Vector3 _forwardAfterTurn = Vector3.forward;

    // Constants
    private const float Skin = 0.01f;

    public PhysicsMover(
        float moveSpeed,
        float steerAngle,
        float traction,
        float handbrakeForwardDecel,
        float handbrakeLateralBoost,
        float handbrakeTractionScale,
        float maxPlanarSpeed,
        float baseLateralFriction)
    {
        _moveSpeed = moveSpeed;
        _steerAngle = steerAngle;
        _traction = Mathf.Max(0f, traction);
        _handbrakeForwardDecel = Mathf.Max(0f, handbrakeForwardDecel);
        _handbrakeLateralBoost = Mathf.Max(0f, handbrakeLateralBoost);
        _handbrakeTractionScale = Mathf.Clamp(handbrakeTractionScale, 0.05f, 1f);
        _maxPlanarSpeed = Mathf.Max(0f, maxPlanarSpeed);
        _baseLateralFriction = Mathf.Max(0f, baseLateralFriction);
    }

    public Vector3 PlanarVelocity => _moveForce; // planer "velocity" in your model
    public float CurrentSpeed => _moveForce.magnitude;
    public Vector3 ForwardAfterTurn => _forwardAfterTurn;

    public void Step(Rigidbody rb, float vInput, float hInput, bool handbrakeHeld, float dt)
    {
        // Accumulate forward force like your original math
        _moveForce += rb.rotation * Vector3.forward * (_moveSpeed * vInput * dt);

        // Steering proportional to speed
        float turnAngle = _moveForce.magnitude * _steerAngle * dt;
        Quaternion nextRotation = rb.rotation * Quaternion.AngleAxis(turnAngle * hInput, Vector3.up);
        rb.MoveRotation(nextRotation);

        // Traction (scaled while braking)
        _forwardAfterTurn = nextRotation * Vector3.forward;

        if (_moveForce.sqrMagnitude > 1e-7f)
        {
            float effectiveTraction = handbrakeHeld ? _traction * _handbrakeTractionScale : _traction;
            _moveForce = Vector3.Lerp(_moveForce.normalized, _forwardAfterTurn, effectiveTraction * dt) * _moveForce.magnitude;
        }

        // Optional lateral friction baseline
        if (_baseLateralFriction > 0f && _moveForce.sqrMagnitude > 1e-7f)
        {
            Vector3 right = Vector3.Cross(Vector3.up, _forwardAfterTurn).normalized;
            Vector3 lateral = Vector3.Project(_moveForce, right);
            _moveForce -= lateral * Mathf.Clamp01(_baseLateralFriction * dt);
        }

        // Handbrake shaping
        if (handbrakeHeld && _moveForce.sqrMagnitude > 1e-7f)
        {
            // Forward deceleration
            float sp = _moveForce.magnitude;
            sp = Mathf.Max(0f, sp - _handbrakeForwardDecel * dt);
            _moveForce = _moveForce.normalized * sp;

            // Extra lateral bleed
            Vector3 right = Vector3.Cross(Vector3.up, _forwardAfterTurn).normalized;
            Vector3 lateral = Vector3.Project(_moveForce, right);
            _moveForce -= lateral * Mathf.Clamp01(_handbrakeLateralBoost * dt);
        }

        // Cap speed for stability (0 disables)
        if (_maxPlanarSpeed > 0f)
        {
            float sp = _moveForce.magnitude;
            if (sp > _maxPlanarSpeed) _moveForce *= (_maxPlanarSpeed / sp);
        }

        // Swept move with slide (two passes for corner reliability)
        Vector3 remaining = _moveForce * dt;
        int passes = 0;
        while (remaining.sqrMagnitude > 1e-8f && passes < 2)
        {
            float dist = remaining.magnitude;
            Vector3 dir = remaining / Mathf.Max(dist, 1e-6f);

            if (rb.SweepTest(dir, out RaycastHit hit, dist + Skin, QueryTriggerInteraction.Ignore))
            {
                float allowed = Mathf.Max(0f, hit.distance - Skin);
                rb.MovePosition(rb.position + dir * allowed);

                // Slide along contact
                _moveForce = Vector3.ProjectOnPlane(_moveForce, hit.normal);
                float into = Vector3.Dot(_moveForce, hit.normal);
                if (into < 0f) _moveForce -= hit.normal * into;

                float timeRatio = 1f - (allowed / Mathf.Max(dist, 1e-6f));
                remaining = _moveForce * (dt * timeRatio);
                passes++;
            }
            else
            {
                rb.MovePosition(rb.position + remaining);
                break;
            }
        }
    }
}

