/*
 * Copyright 2024 Haply Robotics Inc. All rights reserved.
 */

using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

namespace Haply.Samples.Tutorials._6_VerseGripPositionControl
{
    /// <summary>
    /// Demonstrates how to control the device cursor position using the VerseGrip.
    /// </summary>
    public class VerseGripPositionControl : MonoBehaviour
    {
        public Inverse3Controller inverse3;
        public VerseGripController verseGrip;

        [Tooltip("Cursor moving speed")]
        [Range(0, 1)]
        public float speed = 0.5f;

        [Tooltip("Maximum radius for cursor movement")]
        [Range(0, 0.1f)]
        public float movementLimitRadius = 0.075f;

        private Vector3 _targetPosition; // Target position for the cursor

        private void Awake()
        {
            inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
            verseGrip ??= FindFirstObjectByType<VerseGripController>();
            inverse3.Ready.AddListener((inverse3Controller, args) =>
            {
                _targetPosition = inverse3Controller.WorkspaceCenterLocalPosition;
            });
        }

        /// <summary>
        /// Subscribes to the DeviceStateChanged event.
        /// </summary>
        private void OnEnable()
        {
            verseGrip.DeviceStateChanged += OnDeviceStateChanged;
        }

        /// <summary>
        /// Unsubscribes from the DeviceStateChanged event.
        /// </summary>
        private void OnDisable()
        {
            verseGrip.DeviceStateChanged -= OnDeviceStateChanged;
            inverse3.Release();
        }

        private void OnDeviceStateChanged(object sender, VerseGripEventArgs args)
        {
            var verseGrip = args.DeviceController;
            // Calculate the direction based on the VerseGrip's rotation
            var direction = verseGrip.Orientation * Vector3.forward;

            // Check if the VerseGrip button is pressed down
            if (verseGrip.GetButtonDown())
            {
                // Initialize target position
                _targetPosition = inverse3.CursorLocalPosition;
            }

            // Check if the VerseGrip button is being held down
            if (verseGrip.GetButton())
            {
                // Move the target position toward the grip direction
                _targetPosition += direction * (0.0001f * speed);

                // Clamp the target position within the movement limit radius
                var workspaceCenter = inverse3.WorkspaceCenterLocalPosition;
                _targetPosition = Vector3.ClampMagnitude(_targetPosition - workspaceCenter, movementLimitRadius)
                    + workspaceCenter;
            }
            // Move cursor to new position
            inverse3.SetCursorLocalPosition(_targetPosition);
        }

        # region Optional GUI Display and Gizmos
        // --------------------
        // Optional GUI Display
        // --------------------

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(inverse3.WorkspaceCenterLocalPosition, movementLimitRadius + inverse3.Cursor.Radius); // Draw movement limit
        }

        private void OnGUI()
        {
            const float width = 600;
            const float height = 60;
            var rect = new Rect((Screen.width - width) / 2, Screen.height - height - 10, width, height);

            var text = verseGrip.GetButton()
                ? "Rotate the VerseGrip to change the cursor's movement direction."
                : "Press and hold the VerseGrip button to move the cursor in the pointed direction.";

            GUI.Box(rect, text, CenteredStyle());
        }

        private static GUIStyle CenteredStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                },
                fontSize = 14
            };
            return style;
        }

        #endregion
    }
}
