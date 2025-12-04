/*
 * Haply 3.2.0 기반 물체별 촉감 적용 + 멀티스레드 안전 디버깅 최종 버전
 */

using System.Collections.Generic;
using System.Threading;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

namespace Haply.Samples.Experimental.HapticsAndPhysicsEngine
{
    public class PhysicsHapticEffector : MonoBehaviour
    {
        // HAPTICS
        [Header("Haptics")]
        public bool forceEnabled;

        [SerializeField] private float defaultStiffness = 400f;
        [SerializeField] private float defaultDamping = 1f;

        private HapticMaterial currentMaterial;


        // PHYSICS
        [Header("Physics")]
        [SerializeField] private float drag = 20f;
        [SerializeField] private float linearLimit = 0.001f;
        [SerializeField] private float limitSpring = 500000f;
        [SerializeField] private float limitDamper = 10000f;

        private ConfigurableJoint _joint;
        private Rigidbody _rigidbody;


        // --- Thread-safe cached data ---
        private struct PhysicsCursorData
        {
            public Vector3 position;
            public bool collision;
        }

        private PhysicsCursorData _cachedPhysicsCursorData;
        private readonly ReaderWriterLockSlim _cacheLock = new();


        private PhysicsCursorData GetSceneData()
        {
            _cacheLock.EnterReadLock();
            try { return _cachedPhysicsCursorData; }
            finally { _cacheLock.ExitReadLock(); }
        }

        private void SaveSceneData()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _cachedPhysicsCursorData.position = transform.localPosition;
                _cachedPhysicsCursorData.collision = collisionDetection && touched.Count > 0;
            }
            finally { _cacheLock.ExitWriteLock(); }
        }


        // COLLISION
        [Header("Collision detection")]
        public bool collisionDetection = true;
        public List<Collider> touched = new();

        public Inverse3Controller Inverse3 { get; private set; }


        // --- THREAD-SAFE DEBUG STATE ---
        private volatile bool hapticEventReceived = false;
        private volatile bool hapticForceApplied = false;
        private Vector3 lastForceValue;
        private bool lastCollisionState = false;

        private float nextDebugTime = 0f;


        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            Inverse3 = GetComponentInParent<Inverse3Controller>();

            AttachToInverseCursor();
            SetupCollisionDetection();
        }

        private void OnEnable()
        {
            Inverse3.DeviceStateChanged += OnDeviceStateChanged;
        }

        private void OnDisable()
        {
            Inverse3.DeviceStateChanged -= OnDeviceStateChanged;
        }

        private void FixedUpdate()
        {
            SaveSceneData();
        }


        // 🔥 MAIN THREAD DEBUG LOOP — 1초마다 한 번만 출력
        private void Update()
        {
            if (Time.time >= nextDebugTime)
            {
                nextDebugTime = Time.time + 1f;

                var data = GetSceneData();

                Debug.Log(
                    "[Haptics Debug]\n" +
                    $"- Device Event Received: {hapticEventReceived}\n" +
                    $"- forceEnabled: {forceEnabled}\n" +
                    $"- Last Force: {lastForceValue}\n" +
                    $"- Physics Cursor Pos: {data.position}\n" +
                    $"- Collision: {data.collision}\n" +
                    $"- Touched Count: {touched.Count}\n" +
                    $"- Material Active: {(currentMaterial != null)}\n"
                );

                hapticEventReceived = false;
                hapticForceApplied = false;
            }
        }



        // ──────────────────────────────────────────────
        // Physics Joint
        // ──────────────────────────────────────────────

        private void AttachToInverseCursor()
        {
            var rbCursor = Inverse3.Cursor.gameObject.GetComponent<Rigidbody>();
            if (!rbCursor)
            {
                rbCursor = Inverse3.Cursor.gameObject.AddComponent<Rigidbody>();
                rbCursor.useGravity = false;
                rbCursor.isKinematic = true;
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!_rigidbody)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = false;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            _joint = gameObject.GetComponent<ConfigurableJoint>();
            if (!_joint)
                _joint = gameObject.AddComponent<ConfigurableJoint>();

            _joint.connectedBody = rbCursor;
            _joint.autoConfigureConnectedAnchor = false;
            _joint.anchor = Vector3.zero;
            _joint.connectedAnchor = Vector3.zero;

            _joint.xMotion = _joint.yMotion = _joint.zMotion = ConfigurableJointMotion.Limited;
            _joint.angularXMotion = _joint.angularYMotion = _joint.angularZMotion = ConfigurableJointMotion.Locked;

            _joint.linearLimit = new SoftJointLimit() { limit = linearLimit };
            _joint.linearLimitSpring = new SoftJointLimitSpring() { spring = limitSpring, damper = limitDamper };

            _rigidbody.drag = drag;
        }


        // ──────────────────────────────────────────────
        // Force Calculation
        // ──────────────────────────────────────────────

        private Vector3 ForceCalculation(Vector3 hapticCursorPos, Vector3 hapticCursorVel, Vector3 physicsCursorPos)
        {
            float stiffness = currentMaterial ? currentMaterial.stiffness : defaultStiffness;
            float damping = currentMaterial ? currentMaterial.damping : defaultDamping;

            var force = (physicsCursorPos - hapticCursorPos) * stiffness;
            force -= hapticCursorVel * damping;

            lastForceValue = force;
            hapticForceApplied = true;

            return force;
        }


        // ──────────────────────────────────────────────
        // Collision
        // ──────────────────────────────────────────────

        private void SetupCollisionDetection()
        {
            if (!TryGetComponent(out Collider col))
                col = gameObject.AddComponent<SphereCollider>();

            if (!col.material)
                col.material = new PhysicMaterial() { dynamicFriction = 0, staticFriction = 0 };
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (!touched.Contains(collision.collider))
                touched.Add(collision.collider);

            currentMaterial = collision.collider.GetComponent<HapticMaterial>();
        }


        private void OnCollisionExit(Collision collision)
        {
            if (touched.Contains(collision.collider))
                touched.Remove(collision.collider);

            if (touched.Count == 0)
                currentMaterial = null;
        }


        // ──────────────────────────────────────────────
        // DEVICE EVENT (runs on haptic thread)
        // ──────────────────────────────────────────────

        private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
        {
            hapticEventReceived = true;

            var inverse3 = args.DeviceController;
            var physicsData = GetSceneData();

            if (!forceEnabled || (collisionDetection && !physicsData.collision))
            {
                inverse3.SetCursorLocalForce(Vector3.zero);
                return;
            }

            var force = ForceCalculation(
                inverse3.CursorLocalPosition,
                inverse3.CursorLocalVelocity,
                physicsData.position
            );

            inverse3.SetCursorLocalForce(force);
        }
    }
}
