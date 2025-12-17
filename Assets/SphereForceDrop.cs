using System.Threading;
using Haply.Inverse.DeviceControllers;
using UnityEngine;

public class SphereForceDrop : MonoBehaviour, IHapticEffector
{
    public Inverse3Controller inverse3;

    [Header("Elastic Tissue")]
    [Range(0, 800)]
    public float stiffness = 300f;

    [Range(0, 3)]
    public float damping = 1f;

    [Header("Breakthrough")]
    [Tooltip("Force threshold (N) to trigger tissue rupture")]
    public float breakThreshold = 2f;

    [Tooltip("Force multiplier after break (0~1)")]
    public float dropRatio = 1f;

    [Tooltip("If true, rupture resets when the cursor leaves the sphere")]
    public bool resetOnExit = true;

    // rupture state (haptic thread에서도 읽고/쓰므로 volatile)
    private volatile bool _isRuptured = false;

    private struct Cache
    {
        public Vector3 centerLocal;
        public float radius;
    }

    private Cache _cached;
    private readonly ReaderWriterLockSlim _lock = new();

    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
        inverse3.Ready.AddListener((dev, args) => SaveCache());
    }

    private void FixedUpdate()
    {
        if (inverse3 != null && inverse3.IsReady)
            SaveCache();
    }

    private void SaveCache()
    {
        _lock.EnterWriteLock();
        try
        {
            _cached.centerLocal = inverse3.transform.InverseTransformPoint(transform.position);
            _cached.radius = transform.lossyScale.x * 0.5f;
        }
        finally { _lock.ExitWriteLock(); }
    }

    private Cache GetCache()
    {
        _lock.EnterReadLock();
        try { return _cached; }
        finally { _lock.ExitReadLock(); }
    }

    public Vector3 ComputeForce(in Vector3 cursorLocalPos, in Vector3 cursorLocalVel, float cursorRadius)
    {
        var c = GetCache();

        Vector3 distanceVector = cursorLocalPos - c.centerLocal;
        float distance = distanceVector.magnitude;
        float penetration = c.radius + cursorRadius - distance;

        // 접촉 해제 시 rupture 리셋 옵션
        if (penetration <= 0f)
        {
            if (resetOnExit) _isRuptured = false;
            return Vector3.zero;
        }

        Vector3 normal = (distance > 1e-6f) ? (distanceVector / distance) : Vector3.forward;

        // 기본 힘
        Vector3 force = normal * penetration * stiffness;
        force -= cursorLocalVel * damping;

        float forceMag = force.magnitude;
        Debug.Log($"Computed force magnitude: {forceMag}");

        // 한 번 임계값 넘으면 rupture 상태로 고정
        if (!_isRuptured && forceMag >= breakThreshold)
        {
            _isRuptured = true;
        }

        // rupture 이후엔 계속 drop 상태 유지
        if (_isRuptured)
        {
            // force *= dropRatio;
            force = Vector3.zero; // 더 이상 힘 없음 → 그냥 통과
            Debug.Log("Tissue ruptured! Applying reduced force.");
        }

        return force;
    }
}
