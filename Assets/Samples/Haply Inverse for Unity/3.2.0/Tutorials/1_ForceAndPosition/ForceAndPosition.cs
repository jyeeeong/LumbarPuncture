/*
 * Copyright 2024 Haply Robotics Inc. All rights reserved.
 */

using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

namespace Haply.Samples.Tutorials._1_ForceAndPosition
{
    /// <summary>
    /// Demonstrates the application of force to maintain the cursor at the center of the workspace.
    /// </summary>
    public class ForceAndPosition : MonoBehaviour
    {
        public Inverse3Controller inverse3;

        [Range(0, 400)]
        // Stiffness of the force feedback.
        public float stiffness = 100;

        private void Awake()
        {
            inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
        }

        /// <summary>
        /// Subscribes to the DeviceStateChanged event when the component is enabled.
        /// </summary>
        protected void OnEnable()
        {
            inverse3.DeviceStateChanged += OnDeviceStateChanged;
        }

        /// <summary>
        /// Unsubscribes from the DeviceStateChanged event and reset the force when the component is disabled.
        /// </summary>
        protected void OnDisable()
        {
            inverse3.DeviceStateChanged -= OnDeviceStateChanged;
            inverse3.Release();
        }

        /// <summary>
        /// Event handler that calculates and send the force to the device when the cursor's position changes.
        /// </summary>
        /// <param name="sender">The Inverse3 data object.</param>
        /// <param name="args">The event arguments containing the device data.</param>
        private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
        {
            var inverse3 = args.DeviceController;

            // Calculate the force to apply to the cursor.
            var force = inverse3.WorkspaceCenterLocalPosition - inverse3.CursorLocalPosition;

            // Calculate the stiffness force.
            force *= stiffness;

            // Apply the force to the cursor.
            inverse3.SetCursorLocalForce(force);
        }
    }
}
