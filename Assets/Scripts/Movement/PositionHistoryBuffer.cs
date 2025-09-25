using UnityEngine;

/// <summary>
/// Fixed-capacity ring buffer that records positions at a fixed cadence,
/// preserving your "head to last-sample" interpolation logic without Insert(0) costs.
/// </summary>
public sealed class PositionHistoryBuffer
{
    private Vector3[] _buf = System.Array.Empty<Vector3>();
    private int _head;   // index of most-recent recorded sample
    private int _count;  // number of valid samples
    private float _accumulator;
    private float _lastRecordWallTime;

    public int Count => _count;
    public float HeadToRecordDelta => Mathf.Max(1e-6f, Time.time - _lastRecordWallTime);

    public void Initialize(int capacity, Vector3 initialPosition)
    {
        capacity = Mathf.Max(2, capacity);
        _buf = new Vector3[capacity];
        _head = 0;
        _count = 1;
        _buf[_head] = initialPosition;
        _accumulator = 0f;
        _lastRecordWallTime = Time.time;
    }

    public void AccumulateAndRecord(float interval, float dt, Vector3 currentHeadPosition)
    {
        _accumulator += dt;
        while (_accumulator >= interval)
        {
            Push(currentHeadPosition);
            _accumulator -= interval;
        }
    }

    private void Push(Vector3 pos)
    {
        _head = (_head + 1) % _buf.Length;
        _buf[_head] = pos;
        _count = Mathf.Min(_count + 1, _buf.Length);
        _lastRecordWallTime = Time.time;
    }

    /// <summary>
    /// Gets the sample 'k' steps back from the most recent recorded sample (k = 0 => most recent recorded, NOT current head transform).
    /// Returns false if out of range.
    /// </summary>
    public bool TryGetSample(int k, out Vector3 sample)
    {
        if (k < 0 || k >= _count)
        {
            sample = default;
            return false;
        }
        int idx = _head - k;
        if (idx < 0) idx += _buf.Length;
        sample = _buf[idx];
        return true;
    }
}
