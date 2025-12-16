// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class CubeForceEffector : MonoBehaviour
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
using System.Collections.Generic;
using System.Threading;
using Haply.Inverse.DeviceControllers;
using UnityEngine;

/// <summary>
/// Physics 기반(조인트-스프링) 힘 생성 Effector.
/// - 이 스크립트는 "프록시(physics effector)" 오브젝트에 붙인다.
/// - 큐브/뼈/조직은 Collider만 있으면 된다. (장애물 역할)
/// - HapticForceManager가 ComputeForce()를 호출해 힘을 합산한다.
/// </summary>
public class CubeForceEffector : MonoBehaviour, IHapticEffector
{
    [Header("Refs")]
    public Inverse3Controller inverse3;

    [Header("Force Model")]
    [Range(0, 800)] public float stiffness = 400f;
    [Range(0, 3)]   public float damping = 1f;

    [Header("Physics Proxy Settings")]
    [Tooltip("Proxy Rigidbody drag (stabilize joint)")]
    public float proxyDrag = 20f;

    [Tooltip("Joint linear limit (meters) - 작을수록 커서에 더 '붙음'")]
    public float linearLimit = 0.001f;

    [Tooltip("Joint spring (higher = stiffer link)")]
    public float limitSpring = 500000f;

    [Tooltip("Joint damper (higher = less oscillation)")]
    public float limitDamper = 10000f;

    [Header("Collision Gate")]
    [Tooltip("충돌 중일 때만 힘을 내서, 공중에서 스프링 드래그 느낌을 줄임")]
    public bool collisionDetection = true;

    [Tooltip("특정 Collider만 반응하고 싶으면 Tag로 제한 (비우면 전체)")]
    public string targetTag = "Cube"; // 예: "SpinalCord"

    // ───────────── 내부: joint & rigidbody ─────────────
    private ConfigurableJoint _joint;
    private Rigidbody _proxyRb;

    // ───────────── 충돌 상태 ─────────────
    private readonly List<Collider> _touched = new();

    // ───────────── Thread-safe cache (haptic thread에서 Unity API 금지) ─────────────
    private struct Cache
    {
        public Vector3 proxyPosInvLocal; // proxy position in Inverse3 local
        public bool collision;
    }

    private Cache _cached;
    private readonly ReaderWriterLockSlim _lock = new();

    private Cache GetCache()
    {
        _lock.EnterReadLock();
        try { return _cached; }
        finally { _lock.ExitReadLock(); }
    }

    private void SaveCache()
    {
        _lock.EnterWriteLock();
        try
        {
            // Unity main thread에서만 Transform 접근 가능!
            _cached.proxyPosInvLocal = inverse3.transform.InverseTransformPoint(transform.position);
            _cached.collision = collisionDetection ? (_touched.Count > 0) : true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // ───────────── Unity lifecycle ─────────────
    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();

        SetupProxyCollider();
        AttachProxyToInverseCursor();
    }

    private void Start()
    {
        // Inverse3 준비되면 초기 캐시 1회
        inverse3.Ready.AddListener((dev, args) => SaveCache());
    }

    // private void FixedUpdate()
    // {
    //     if (inverse3 != null && inverse3.IsReady)
    //         SaveCache();
    // }
    private void FixedUpdate()
    {
        if (inverse3 == null || !inverse3.IsReady) return;

        SaveCache();

        // 충돌이 없으면 proxy를 cursor 위치로 당겨 붙이기 (air-drag 제거)
        if (collisionDetection && _touched.Count == 0)
        {
            // cursor world position에 proxy를 스냅
            var cursorWorld = inverse3.Cursor.transform.position;
            _proxyRb.position = cursorWorld;
            _proxyRb.velocity = Vector3.zero;
        }
    }


    // ───────────── Setup ─────────────
    private void SetupProxyCollider()
    {
        // 프록시는 충돌을 "받아야" 하므로 Collider + non-kinematic Rigidbody 필요
        if (!TryGetComponent(out Collider col))
            col = gameObject.AddComponent<SphereCollider>();

        if (!col.material)
            col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };

        _proxyRb = GetComponent<Rigidbody>();
        if (!_proxyRb)
            _proxyRb = gameObject.AddComponent<Rigidbody>();

        _proxyRb.useGravity = false;
        _proxyRb.isKinematic = false;
        _proxyRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _proxyRb.drag = proxyDrag;
    }

    private void AttachProxyToInverseCursor()
    {
        // Cursor에 kinematic Rigidbody 보장
        var cursorRb = inverse3.Cursor.gameObject.GetComponent<Rigidbody>();
        if (!cursorRb)
            cursorRb = inverse3.Cursor.gameObject.AddComponent<Rigidbody>();

        cursorRb.useGravity = false;
        cursorRb.isKinematic = true;

        // Proxy ↔ Cursor 를 joint로 연결
        _joint = GetComponent<ConfigurableJoint>();
        if (!_joint) _joint = gameObject.AddComponent<ConfigurableJoint>();

        _joint.connectedBody = cursorRb;
        _joint.autoConfigureConnectedAnchor = false;
        _joint.anchor = Vector3.zero;
        _joint.connectedAnchor = Vector3.zero;

        // linear만 제한, 회전은 잠금(구가 굴러서 이상한 느낌 방지)
        _joint.xMotion = _joint.yMotion = _joint.zMotion = ConfigurableJointMotion.Limited;
        _joint.angularXMotion = _joint.angularYMotion = _joint.angularZMotion = ConfigurableJointMotion.Locked;

        _joint.linearLimit = new SoftJointLimit { limit = linearLimit };
        _joint.linearLimitSpring = new SoftJointLimitSpring { spring = limitSpring, damper = limitDamper };
    }

    // ───────────── Collision events (main thread) ─────────────
    private void OnCollisionEnter(Collision collision)
    {
        if (!string.IsNullOrEmpty(targetTag) && !collision.collider.CompareTag(targetTag))
            return;

        if (!_touched.Contains(collision.collider))
            _touched.Add(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!string.IsNullOrEmpty(targetTag) && !collision.collider.CompareTag(targetTag))
            return;

        _touched.Remove(collision.collider);
    }

    // ───────────── IHapticEffector ─────────────
    // haptic thread에서 호출됨: UnityEngine.Transform 접근 금지!
    public Vector3 ComputeForce(in Vector3 cursorLocalPos, in Vector3 cursorLocalVel, float cursorRadius)
    {
        var c = GetCache();

        if (collisionDetection && !c.collision)
            return Vector3.zero;

        // Physics proxy가 "막혀서" cursor를 따라가지 못한 차이 = 스프링 변위
        Vector3 force = (c.proxyPosInvLocal - cursorLocalPos) * stiffness;

        // 공중에서의 드래그 느낌을 줄이려면 damping은 cursor velocity 기준으로 적용
        force -= cursorLocalVel * damping;

        return force;
    }
}
