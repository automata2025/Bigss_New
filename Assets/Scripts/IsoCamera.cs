using UnityEngine;

[DisallowMultipleComponent]
public class IsoCamera : MonoBehaviour
{
    private enum ProjectionMode { Perspective, Orthographic }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float targetHeight = 1.0f;

    [Header("Framing")]
    [SerializeField, Range(0f, 360f)] private float yaw = 45f;
    [SerializeField, Range(15f, 75f)] private float pitch = 35f;

    [Header("Zoom / Distance")]
    [SerializeField] private ProjectionMode mode = ProjectionMode.Perspective;
    [SerializeField] private float baseZoom = 12f;   // distance (Perspective) or size (Ortho)
    [SerializeField] private float minZoom = 6f;
    [SerializeField] private float maxZoom = 28f;
    [SerializeField] private bool enableMouseWheelZoom = true;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField, Range(0.01f, 0.5f)] private float zoomSmooth = 0.12f;

    [Header("Follow Smoothing")]
    [SerializeField, Range(0.01f, 0.6f)] private float followSmooth = 0.12f;
    [SerializeField] private float maxDeltaTime = 0.033f; // smoothing clamp (~30fps)

    [Header("Collision (Optional)")]
    [SerializeField] private bool avoidObstacles = true;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField, Range(0.05f, 0.75f)] private float collisionRadius = 0.25f;
    [SerializeField, Range(0f, 0.3f)] private float collisionSkin = 0.05f;
    [SerializeField, Range(0.01f, 0.5f)] private float collisionSmooth = 0.08f;

    // --- internals ---
    private Camera cam;
    private Vector3 followPos, followVel;
    private float zoomCurr, zoomVel;
    private float collisionDepth, collisionDepthVel;
    private bool initialized;

    // --- Properties for other scripts (optional use) ---
    public Transform Target { get => target; set => target = value; }
    public float Yaw { get => yaw; set => yaw = value; }
    public float Pitch { get => pitch; set => pitch = Mathf.Clamp(value, 15f, 75f); }
    public float Zoom
    {
        get => zoomCurr;
        set => zoomCurr = Mathf.Clamp(value, minZoom, maxZoom);
    }
    public bool Orthographic
    {
        get => mode == ProjectionMode.Orthographic;
        set => mode = value ? ProjectionMode.Orthographic : ProjectionMode.Perspective;
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
        {
            cam = GetComponentInChildren<Camera>(true);
            if (!cam)
            {
                Debug.LogError($"{nameof(IsoCamera)}: No Camera found.");
                enabled = false;
                return;
            }
        }

        cam.orthographic = (mode == ProjectionMode.Orthographic);
        zoomCurr = Mathf.Clamp(baseZoom, minZoom, maxZoom);
    }

    private void LateUpdate()
    {
        if (!target || !cam) return;

        float dt = Mathf.Min(Time.deltaTime, maxDeltaTime);

        // --- Follow (critically damped) ---
        Vector3 anchor = target.position + Vector3.up * targetHeight;
        if (!initialized)
        {
            followPos = anchor;
            initialized = true;
        }
        else
        {
            followPos = Vector3.SmoothDamp(followPos, anchor, ref followVel, followSmooth, Mathf.Infinity, dt);
        }

        // --- Zoom input (optional) ---
        float zoomGoal = zoomCurr;
        if (enableMouseWheelZoom)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
                zoomGoal = Mathf.Clamp(zoomCurr - scroll * zoomSpeed, minZoom, maxZoom);
        }
        zoomCurr = Mathf.SmoothDamp(zoomCurr, zoomGoal, ref zoomVel, zoomSmooth, Mathf.Infinity, dt);

        // --- Orientation / desired offset ---
        Quaternion isoRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 forward = isoRot * Vector3.forward;
        Vector3 back = -forward;

        // --- Compute camera placement ---
        if (mode == ProjectionMode.Perspective)
        {
            float desiredDist = zoomCurr;
            float actualDist = desiredDist;

            if (avoidObstacles)
                actualDist = ResolveCollisionDistance(followPos, back, desiredDist, dt);

            Vector3 camPos = followPos + back * actualDist;
            transform.position = camPos;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            cam.orthographic = false; // ensure perspective
        }
        else
        {
            // Orthographic uses size as zoom; we still place the camera backward for parallax-free iso
            cam.orthographic = true;
            cam.orthographicSize = zoomCurr;

            float physicalDist = Mathf.Lerp(8f, 24f, Mathf.InverseLerp(minZoom, maxZoom, zoomCurr));
            if (avoidObstacles)
                physicalDist = ResolveCollisionDistance(followPos, back, physicalDist, dt);

            Vector3 camPos = followPos + back * physicalDist;
            transform.position = camPos;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }

    private float ResolveCollisionDistance(Vector3 pivot, Vector3 backDir, float desiredDist, float dt)
    {
        float castDist = Mathf.Max(0.0001f, desiredDist);
        if (Physics.SphereCast(pivot, collisionRadius, backDir, out RaycastHit hit, castDist + collisionSkin, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float permitted = Mathf.Max(0f, hit.distance - collisionSkin);
            float depth = 1f - (permitted / castDist); // 0..1 (0 = clear, 1 = fully blocked)
            collisionDepth = Mathf.SmoothDamp(collisionDepth, Mathf.Clamp01(depth), ref collisionDepthVel, collisionSmooth, Mathf.Infinity, dt);
        }
        else
        {
            collisionDepth = Mathf.SmoothDamp(collisionDepth, 0f, ref collisionDepthVel, collisionSmooth, Mathf.Infinity, dt);
        }

        // Lerp toward the surface when obstructed
        float minDist = Mathf.Max(0.1f, collisionRadius + 0.01f);
        return Mathf.Lerp(desiredDist, minDist, collisionDepth);
    }
}
