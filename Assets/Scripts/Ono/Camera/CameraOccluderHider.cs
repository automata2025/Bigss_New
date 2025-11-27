using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOccluderHider : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public float targetHeight = 1.0f; 

    public LayerMask occluderMask;
    public float checkRadius = 0.3f;

    private readonly List<Renderer> _hidden = new List<Renderer>();

    private void LateUpdate()
    {
        if (!target) return;

        for (int i = 0; i < _hidden.Count; i++)
        {
            if (_hidden[i] != null)
                _hidden[i].enabled = true;
        }
        _hidden.Clear();

        Vector3 from = transform.position;
        Vector3 to = target.position + Vector3.up * targetHeight;
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.01f) return;
        dir /= dist;

        RaycastHit[] hits = Physics.SphereCastAll(
            from,
            checkRadius,
            dir,
            dist,
            occluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var rend = hit.collider.GetComponentInChildren<Renderer>();
            if (rend != null && rend.enabled)
            {
                rend.enabled = false;
                _hidden.Add(rend);
            }
        }
    }
}

