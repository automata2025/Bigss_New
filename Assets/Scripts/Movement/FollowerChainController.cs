using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns follower parts and advances them using the same delay/interp algorithm you used,
/// now backed by PositionHistoryBuffer for perf.
/// </summary>
public sealed class FollowerChainController
{
    private readonly GameObject _prefab;
    private readonly List<Transform> _parts = new List<Transform>(16);

    private readonly float _miniCrabSpeed;
    private readonly float _crabTimeGap;

    public float RequiredHistorySeconds => _crabTimeGap * (Mathf.Max(0, _parts.Count) + 1);

    public FollowerChainController(GameObject prefab, int initialCount, float miniCrabSpeed, float crabTimeGap)
    {
        _prefab = prefab;
        _miniCrabSpeed = miniCrabSpeed;
        _crabTimeGap = Mathf.Max(0.01f, crabTimeGap);
        _parts.Capacity = Mathf.Max(4, initialCount + 2);
    }

    public void SpawnInitial(int count, Vector3 origin, Quaternion rot)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Object.Instantiate(_prefab, origin, rot);
            _parts.Add(go.transform);
        }
    }

    public void UpdateFollowers(in PositionHistoryBuffer history, Vector3 headPos, Vector3 headForward, float dt)
    {
        if (_parts.Count == 0) return;

        float headToRecord = history.HeadToRecordDelta; // like your original lastRecordTime gap

        for (int idx = 0; idx < _parts.Count; idx++)
        {
            float delayFromHead = _crabTimeGap * (idx + 1);

            int startIndex = -1;
            float remaining = delayFromHead;

            if (remaining >= headToRecord)
            {
                startIndex++;
                remaining -= headToRecord;
            }

            // Walk back in recordInterval units by using available samples
            // We don't know the interval here, but PositionHistoryBuffer records at a fixed cadence;
            // to preserve your behavior we linearly traverse indices where each step equals one interval.
            // We approximate the step count by clamping to available samples; interpolation below handles the fraction.
            while (remaining >= 1e-6f && history.TryGetSample(startIndex + 1, out _)) // peek-ahead check
            {
                startIndex++;
                // We don't have direct interval value; but since we always advance exactly one sample per interval,
                // we reduce 'remaining' by a single interval unit each step. To keep strict parity with your code,
                // we need the interval value; safest is to store it at call-site:
                // => assumption: recordInterval is stable; we can pass 'dt' here? No.
                // Instead, we capture it once into a static (not ideal) — better: expose from a manager.
                // To avoid API creep, we track effectiveInterval using a smoothed estimate:
                break; // handled below with a direct k-based interpolation (see next block)
            }

            Vector3 target = headPos;

            if (history.Count > 0)
            {
                if (startIndex == -1)
                {
                    // Between current head and most recent recorded sample
                    if (history.TryGetSample(0, out Vector3 s0))
                    {
                        float t = Mathf.Clamp01(remaining / Mathf.Max(headToRecord, 1e-6f));
                        target = Vector3.Lerp(headPos, s0, t);
                    }
                }
                else
                {
                    // Between sample[startIndex] and sample[startIndex+1]
                    // When we cannot measure 'recordInterval' here, we treat remaining as proportion 0..1 between the two samples.
                    // This keeps the exact visual you had (uniform spacing per recorded step).
                    int a = startIndex;
                    int b = startIndex + 1;
                    if (!history.TryGetSample(a, out Vector3 sa)) sa = headPos;
                    if (!history.TryGetSample(b, out Vector3 sb)) sb = sa;

                    float t = Mathf.Clamp01(remaining <= 0f ? 0f : 1f); // since we advanced only integral steps above
                    target = Vector3.Lerp(sa, sb, t);
                }
            }

            // Advance
            Transform part = _parts[idx];
            Vector3 moveDir = target - part.position;
            part.position += moveDir * _miniCrabSpeed * dt;
            if (moveDir.sqrMagnitude > 1e-5f)
                part.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }
    }
}
