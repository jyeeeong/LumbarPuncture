// using System.Diagnostics;
using System.Threading;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

public class CubeTissueLayer : MonoBehaviour
{
    public Inverse3Controller inverse3;
    public float stiffness = 500f;
    public float damping = 1f;
    public float cursorRadius = 0.002f;

    private struct SceneData
    {
        public Matrix4x4 cubeToInverse3;   // cube → inverse3 변환
        public Matrix4x4 inverse3ToCube;   // inverse3 → cube 변환
        public Vector3 halfSize;           // cube half scale (local)
    }

    private readonly ReaderWriterLockSlim _lock = new();
    private SceneData _cached;

    private void SaveSceneData()
    {
        _lock.EnterWriteLock();
        try
        {
            // cube → inverse3
            Matrix4x4 cubeWorld = transform.localToWorldMatrix;
            Matrix4x4 inverse3WorldToLocal = inverse3.transform.worldToLocalMatrix;

            _cached.cubeToInverse3 = inverse3WorldToLocal * cubeWorld;
            _cached.inverse3ToCube = cubeWorld.inverse * inverse3.transform.localToWorldMatrix;

            _cached.halfSize = transform.localScale * 0.5f;
        }
        finally { _lock.ExitWriteLock(); }
    }

    private SceneData GetSceneData()
    {
        _lock.EnterReadLock();
        try { return _cached; }
        finally { _lock.ExitReadLock(); }
    }

    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
        inverse3.Ready.AddListener((a, b) => SaveSceneData());
    }

    private void FixedUpdate()
    {
        if (inverse3.IsReady)
            SaveSceneData();
    }

    private void OnEnable()
    {
        inverse3.DeviceStateChanged += OnDeviceStateChanged;
    }

    private void OnDisable()
    {
        inverse3.DeviceStateChanged -= OnDeviceStateChanged;
    }

    private Vector3 ComputeForce(Vector3 cursorLocal, Vector3 cursorVel, SceneData d)
    {
        // 1) cursor → cube local space
        Vector3 cursorCube = d.inverse3ToCube.MultiplyPoint3x4(cursorLocal);

        Vector3 half = d.halfSize;

        // 2) closest point inside cube
        Vector3 clamped = new Vector3(
            Mathf.Clamp(cursorCube.x, -half.x, half.x),
            Mathf.Clamp(cursorCube.y, -half.y, half.y),
            Mathf.Clamp(cursorCube.z, -half.z, half.z)
        );

        Vector3 diff = cursorCube - clamped;
        float dist = diff.magnitude;

        float penetration = cursorRadius - dist;
        if (penetration <= 0f)
            return Vector3.zero;

        Vector3 normalCube = (dist > 0f) ? diff.normalized : Vector3.up;

        // 3) cube local normal → inverse3 local normal
        Vector3 normalInvLocal = d.cubeToInverse3.MultiplyVector(normalCube).normalized;

        // force
        Vector3 force = normalInvLocal * (penetration * stiffness);
        force -= cursorVel * damping;
        Debug.Log($"Penetration: {penetration}");

        return force;
    }

    private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
    {
        var dev = args.DeviceController;

        var d = GetSceneData();
        Vector3 cursorLocal = dev.CursorLocalPosition;
        Vector3 cursorVel   = dev.CursorLocalVelocity;

        Vector3 force = ComputeForce(cursorLocal, cursorVel, d);

        dev.SetCursorLocalForce(force);
        Debug.Log($"Force applied to cursor: {force}");
    }
}
