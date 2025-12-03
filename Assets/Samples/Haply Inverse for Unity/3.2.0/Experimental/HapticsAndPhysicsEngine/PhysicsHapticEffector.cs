/*
 * Copyright 2024 Haply Robotics Inc. All rights reserved.
 */

using System.Collections.Generic;
using System.Threading;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

namespace Haply.Samples.Experimental.HapticsAndPhysicsEngine
{
    /// <summary>
    /// <b>EXPERIMENTAL:</b><br/>
    /// This example demonstrates how to simulate haptic feedback in Unity using its built-in physics engine.
    /// It showcases the limitations of using Unity's FixedUpdate loop and PhysX for real-time haptics.
    ///
    /// <para>
    /// Two effectors are used in this setup:
    /// </para>
    /// <list type="bullet">
    ///     <item><b>Cursor:</b> A kinematic rigidbody, updated directly by the haptic device each frame (~1000Hz).</item>
    ///     <item><b>Physics Effector:</b> A non-kinematic rigidbody linked to the cursor via a spring-damper joint.</item>
    /// </list>
    ///
    /// <para>
    /// Haptic forces are generated based on the distance between the Cursor and the Physics Effector. When the effector
    /// is blocked by objects in the scene, an opposing force is applied through the spring, simulating contact.
    /// Unity's Physics Materials can be used to modulate frictional and material properties.
    /// </para>
    ///
    /// <para><b>Disclaimer:</b></para>
    /// <para>
    /// This approach is for demonstration only. Unity’s physics engine (PhysX) runs at a much lower update rate
    /// (typically 60–120Hz), compared to the haptic thread (~1000Hz). Even though the Fixed Timestep can be set as low
    /// as 0.001s (1kHz), PhysX does not simulate reliably above ~500Hz and may clamp or skip updates under load.
    /// This results in lagging or step-like force feedback that is not suitable for precise haptics.
    /// </para>
    ///
    /// <para>Suggested workarounds:</para>
    /// <list type="bullet">
    ///     <item>Reduce <b>Fixed Timestep</b> in Project Settings to approach 0.001 (1kHz), if performance allows.</item>
    ///     <item>Enable <see cref="collisionDetection"/> to apply forces only on contact and reduce air drag artifacts.</item>
    ///     <item>Use a dedicated haptic/physics middleware (e.g. SOFA, TOIA) for accurate contact simulation.</item>
    /// </list>
    /// </summary>
    public class PhysicsHapticEffector : MonoBehaviour
    {
        // HAPTICS
        [Header("Haptics")]
        [Tooltip("Enable/Disable force feedback")]
        public bool forceEnabled;

        [SerializeField]
        [Range(0, 800)]
        private float stiffness = 400f;

        [SerializeField]
        [Range(0, 3)]
        private float damping = 1;

        // PHYSICS
        [Header("Physics")]

        [SerializeField]
        private float drag = 20f;

        [SerializeField]
        private float linearLimit = 0.001f;

        [SerializeField]
        private float limitSpring = 500000f;

        [SerializeField]
        private float limitDamper = 10000f;

        private ConfigurableJoint _joint;
        private Rigidbody _rigidbody;

        #region Thread-safe cached data

        /// <summary>
        /// Represents scene data that can be updated in the Update() call.
        /// </summary>
        private struct PhysicsCursorData
        {
            public Vector3 position;
            public bool collision;
        }

        /// <summary>
        /// Cached version of the scene data.
        /// </summary>
        private PhysicsCursorData _cachedPhysicsCursorData;

        /// <summary>
        /// Lock to ensure thread safety when reading or writing to the cache.
        /// </summary>
        private readonly ReaderWriterLockSlim _cacheLock = new();

        /// <summary>
        /// Safely reads the cached data.
        /// </summary>
        /// <returns>The cached scene data.</returns>
        private PhysicsCursorData GetSceneData()
        {
            _cacheLock.EnterReadLock();
            try
            {
                return _cachedPhysicsCursorData;
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Safely updates the cached data.
        /// </summary>
        private void SaveSceneData()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _cachedPhysicsCursorData.position = transform.localPosition;
                _cachedPhysicsCursorData.collision = collisionDetection && touched.Count > 0;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        #endregion

        [Header("Collision detection")]
        [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
        public bool collisionDetection;
        public List<Collider> touched = new();

        public Inverse3Controller Inverse3 { get; private set; }

        protected void Awake()
        {
            Inverse3 = GetComponentInParent<Inverse3Controller>();

            // create the physics link between physic effector and device cursor
            AttachToInverseCursor();
            SetupCollisionDetection();
        }

        protected void OnEnable()
        {
            //TODO use world position
            Inverse3.DeviceStateChanged += OnDeviceStateChanged;
        }

        protected void OnDisable()
        {
            Inverse3.DeviceStateChanged -= OnDeviceStateChanged;
        }

        protected void FixedUpdate()
        {
            SaveSceneData();
        }

        //PHYSICS
        #region Physics Joint

        /// <summary>
        /// Attach the current physics effector to device Cursor with a joint
        /// </summary>
        private void AttachToInverseCursor()
        {
            // Add kinematic rigidbody to cursor
            var rbCursor = Inverse3.Cursor.gameObject.GetComponent<Rigidbody>();
            if (!rbCursor)
            {
                rbCursor = Inverse3.Cursor.gameObject.AddComponent<Rigidbody>();
                rbCursor.useGravity = false;
                rbCursor.isKinematic = true;
            }

            // Add non-kinematic rigidbody to self
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!_rigidbody)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = false;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            // Connect with cursor rigidbody with a spring/damper joint and locked rotation
            _joint = gameObject.GetComponent<ConfigurableJoint>();
            if (!_joint)
            {
                _joint = gameObject.AddComponent<ConfigurableJoint>();
            }
            _joint.connectedBody = rbCursor;
            _joint.autoConfigureConnectedAnchor = false;
            _joint.anchor = _joint.connectedAnchor = Vector3.zero;
            _joint.axis = _joint.secondaryAxis = Vector3.zero;

            // limited linear movements
            _joint.xMotion = _joint.yMotion = _joint.zMotion = ConfigurableJointMotion.Limited;

            // lock rotation to avoid sphere roll caused by physics material friction instead of feel it
            _joint.angularXMotion = _joint.angularYMotion = _joint.angularZMotion = ConfigurableJointMotion.Locked;

            // configure limit, spring and damper
            _joint.linearLimit = new SoftJointLimit() { limit = linearLimit };
            _joint.linearLimitSpring = new SoftJointLimitSpring() { spring = limitSpring, damper = limitDamper };

            // stabilize spring connection
            _rigidbody.drag = drag;
        }

        #endregion

        // HAPTICS
        #region Haptics

        /// <summary>
        /// Calculate the force to apply based on the cursor position and the scene data
        /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
        /// </summary>
        /// <param name="hapticCursorPosition">cursor position</param>
        /// <param name="hapticCursorVelocity">cursor velocity</param>
        /// <param name="physicsCursorPosition">physics cursor position</param>
        /// <returns>Force to apply</returns>
        private Vector3 ForceCalculation(Vector3 hapticCursorPosition, Vector3 hapticCursorVelocity,
            Vector3 physicsCursorPosition)
        {
            var force = physicsCursorPosition - hapticCursorPosition;
            force *= stiffness;
            force -= hapticCursorVelocity * damping;
            return force;
        }

        #endregion

        // COLLISION DETECTION
        #region Collision Detection

        private void SetupCollisionDetection()
        {
            // Add collider if not exists
            var col = gameObject.GetComponent<Collider>();
            if (!col)
            {
                col = gameObject.AddComponent<SphereCollider>();
            }

            // Neutral PhysicMaterial to interact with others
            if (!col.material)
            {
                col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };
            }

            collisionDetection = true;
        }

        /// <summary>
        /// Called when effector touch other game object
        /// </summary>
        /// <param name="collision">collision information</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
            {
                // store touched object
                touched.Add(collision.collider);
            }
        }

        /// <summary>
        /// Called when effector move away from another game object
        /// </summary>
        /// <param name="collision">collision information</param>
        private void OnCollisionExit(Collision collision)
        {
            if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
            {
                touched.Remove(collision.collider);
            }
        }

        #endregion

        private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
        {
            var inverse3 = args.DeviceController;
            var physicsCursorData = GetSceneData();
            if (!forceEnabled || (collisionDetection && !physicsCursorData.collision))
            {
                // Don't compute forces if there are no collisions which prevents feeling drag/friction while moving through air.
                inverse3.SetCursorLocalForce(Vector3.zero);
                return;
            }
            var force = ForceCalculation(inverse3.CursorLocalPosition, inverse3.CursorLocalVelocity, physicsCursorData.position);
            inverse3.SetCursorLocalForce(force);
        }
    }
}
