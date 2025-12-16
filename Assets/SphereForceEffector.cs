// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SphereForceEffector : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }

using System.Threading;
using Haply.Inverse.DeviceControllers;
using UnityEngine;

public class SphereForceEffector : MonoBehaviour, IHapticEffector
{
    public Inverse3Controller inverse3;

    [Range(0, 800)]
    public float stiffness = 300f;

    [Range(0, 3)]
    public float damping = 1f;

    private struct Cache
    {
        public Vector3 centerLocal; // inverse3 로컬
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

        if (penetration <= 0f)
            return Vector3.zero;

        Vector3 normal = (distance > 1e-6f) ? (distanceVector / distance) : Vector3.forward;

        Vector3 force = normal * penetration * stiffness;
        force -= cursorLocalVel * damping;
        return force;
    }
}
