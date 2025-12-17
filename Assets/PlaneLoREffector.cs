
using System.Threading;
using Haply.Inverse.DeviceControllers;
using UnityEngine;

public class PlaneLoREffector : MonoBehaviour, IHapticEffector
{
    public Inverse3Controller inverse3;

    [Header("Force")]
    public float stiffness = 600f;
    public float damping = 1f;

    [Header("LoR")]
    public float ruptureForce = 8f;
    public float postRuptureStiffness = 10f;
    public bool resetOnExit = true;

    // cached in Inverse3 local (thread-safe)
    private struct Cache
    {
        public Vector3 p0Local;
        public Vector3 nLocal; // normalized
    }

    private Cache _c;
    private readonly ReaderWriterLockSlim _lock = new();
    private volatile bool ruptured = false;

    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
        inverse3.Ready.AddListener((dev, args) => SaveCache());
    }

    private void FixedUpdate()
    {
        if (inverse3 != null && inverse3.IsReady) SaveCache();
    }

    private void SaveCache()
    {
        _lock.EnterWriteLock();
        try
        {
            // plane point: this object's position
            _c.p0Local = inverse3.transform.InverseTransformPoint(transform.position);

            // plane normal: this object's "up" = 바닥에 수평인 Plane이라면 up이 법선(+Y)
            Vector3 nWorld = transform.up;

            // world normal -> inverse3 local direction
            Vector3 nLocal = inverse3.transform.InverseTransformDirection(nWorld);
            if (nLocal.sqrMagnitude < 1e-12f) nLocal = Vector3.forward;
            _c.nLocal = nLocal.normalized;
        }
        finally { _lock.ExitWriteLock(); }
    }

    private Cache GetCache()
    {
        _lock.EnterReadLock();
        try { return _c; }
        finally { _lock.ExitReadLock(); }
    }

    public Vector3 ComputeForce(in Vector3 cursorPos, in Vector3 cursorVel, float cursorRadius)
    {
        var c = GetCache();
        Vector3 p0 = c.p0Local;
        Vector3 n  = c.nLocal;

        float d = Vector3.Dot(cursorPos - p0, n);

        // plane 앞쪽이면 힘 없음
        if (d > cursorRadius)
        {
            if (resetOnExit) ruptured = false;
            return Vector3.zero;
        }

        float penetration = cursorRadius - d;

        float k = ruptured ? postRuptureStiffness : stiffness;

        Vector3 force = n * penetration * k;
        force -= cursorVel * damping;

        if (!ruptured && force.magnitude >= ruptureForce)
            ruptured = true;

        // Debug.Log($"[PlaneLoR] d={d:F4}, pen={penetration:F4}, ruptured={ruptured}, force={force}");

        return force;
    }
}
