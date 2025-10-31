using Haply.Inverse;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

namespace Haply.Samples.Tutorials._7_DeviceMapping
{
    /// <summary>
    /// This example demonstrates available DeviceMapper methods to manually list, map and connect Haply devices.
    ///
    /// It also gives an example of how to handle the verse grip handedness check and swap the devices between the controllers.
    /// 
    /// Note: This is an example script for tutorial purposes and should not be used in production.
    /// </summary>
    public class DeviceSelector : MonoBehaviour
    {
        private DeviceMapper _deviceMapper;

        public Inverse3Controller leftInverse3;
        public Inverse3Controller rightInverse3;

        private string _message;
        private bool _waitingForVerseGripHandednessConfirm;

        private void Awake()
        {
            _deviceMapper = GetComponent<DeviceMapper>();
            _deviceMapper.autoAssign = false;
            _deviceMapper.autoConnect = false;
            _deviceMapper.autoFetchDeviceList = false;
            _deviceMapper.Ready.AddListener(SetupVerseGripHandednessCheck);
            _deviceMapper.Error += OnError;
            leftInverse3.Ready.AddListener(OnDeviceReady);
            rightInverse3.Ready.AddListener(OnDeviceReady);
        }

        // Start the verse grip handedness check (ask the user to press a button on the right controller)
        private void SetupVerseGripHandednessCheck()
        {
            // Get the VerseGrip controllers using the DeviceMapper (must be done in Start or on DeviceMapper Ready event)
            var leftVerseGrip = _deviceMapper.GetVerseGripController(leftInverse3);
            var rightVerseGrip = _deviceMapper.GetVerseGripController(rightInverse3);
            
            leftVerseGrip.ButtonDown.AddListener(OnButtonDown);
            rightVerseGrip.ButtonDown.AddListener(OnButtonDown);

            // Start waiting for button press on the right controller
            _message = "Press the button on the right controller to confirm handedness.";
            _waitingForVerseGripHandednessConfirm = true;
        }

        // Handle the button down event to confirm the verse grip handedness
        private void OnButtonDown(VerseGripController verseGrip, VerseGripEventArgs args)
        {
            if (_waitingForVerseGripHandednessConfirm && args.Button is VerseGripButton.Button0 or VerseGripButton.Button1)
            {
                if (verseGrip == _deviceMapper.GetVerseGripController(rightInverse3))
                {
                    _message = "VerseGrip handedness confirmed!";
                }
                else
                {
                    _message = "Wrong controller button pressed. Swap the controllers.";
                    _deviceMapper.SwapVerseGrip();
                }
                _waitingForVerseGripHandednessConfirm = false;
            }
            else
            {
                _message = $"Button {args.Button} pressed on {verseGrip.DeviceId}";
            }
        }

        // Handle the device ready event to start probing the cursor position
        private void OnDeviceReady(Inverse3Controller device, Inverse3EventArgs _)
        {
            // Start probing the cursor position when the device is ready
            device.ProbeCursorPosition();
        }

        // Handle errors from the device mapper
        private void OnError(object sender, DeviceMapperErrorEventArgs e)
        {
            _message = e.ErrorMessage;
        }

        // Display the device list
        private void DeviceListGUI()
        {
            if (_deviceMapper.GetNumInverse3() + _deviceMapper.GetNumVerseGrip() > 0)
            {
                GUILayout.Label("Connected devices:");
                foreach (var device in _deviceMapper.GetInverse3Devices())
                {
                    GUILayout.Label($"- {device}");
                }
                foreach (var device in _deviceMapper.GetVerseGripDevices())
                {
                    GUILayout.Label($"- {device}");
                }
            }
        }

        // Display the device mapper state and actions.
        private void DeviceMapperGUI()
        {
            switch (_deviceMapper.State)
            {
                case DeviceMapperState.UNINITIALIZED:
                case DeviceMapperState.INITIALIZED:
                    if (GUILayout.Button(new GUIContent("List devices",
                        "Fetch the device list by HTTP request")))
                    {
                        _deviceMapper.FetchDeviceListOnce();
                        _message = "Fetching device list...";
                    }
                    break;

                case DeviceMapperState.DEVICE_LIST_COMPLETE:
                    if (GUILayout.Button(new GUIContent("Map devices",
                        "Map the devices to the controllers according to the selected handedness")))
                    {
                        _deviceMapper.MapDevices();
                        _message = "Mapping devices...";
                    }
                    break;

                case DeviceMapperState.MAPPING_COMPLETE:
                    if (!_deviceMapper.IsReady)
                    {
                        if (GUILayout.Button(new GUIContent("Connect",
                            "Connect the devices to WebSocket server to receive real-time data")))
                        {
                            _deviceMapper.Connect();
                            _message = "Connecting...";
                        }
                    }
                    break;

                case DeviceMapperState.DEVICE_LIST_IN_PROGRESS:
                case DeviceMapperState.MAPPING_IN_PROGRESS:
                case DeviceMapperState.CONNECTED:
                    var style = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };
                    GUILayout.Label(_message, style);
                    break;

                case DeviceMapperState.ERROR:
                    var errorStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
                    GUILayout.Label(_message, errorStyle);
                    if (GUILayout.Button(new GUIContent("Retry")))
                    {
                        _deviceMapper.Reset();
                    }
                    break;
            }
        }

        // Display the device controller GUI for the inverse3 controller.
        private void Inverse3ControllerGUI(Inverse3Controller controller)
        {
            // Before the device is assigned, we can select the handedness
            if (!controller.Assigned)
            {
                GUILayout.Label("Inverse3Controller \u2192 <not assigned>", GUILayout.Width(800));

                if (GUILayout.Button($"Filter: {controller.SelectedHandedness}"))
                {
                    switch (controller.SelectedHandedness)
                    {
                        case HandednessType.Left:
                            controller.SelectedHandedness = HandednessType.Right;
                            break;
                        case HandednessType.Right:
                            controller.SelectedHandedness = HandednessType.Any;
                            break;
                        case HandednessType.Any:
                            controller.SelectedHandedness = HandednessType.Left;
                            break;
                    }
                }
            }

            // Once the device is assigned, devices can be swapped
            else
            {
                GUILayout.Label($"Inverse3Controller \u2192 #{controller.DeviceId}.{controller.Handedness}", GUILayout.Width(800));

                // Swap the two inverse3 controllers' assigned devices
                if (GUILayout.Button(new GUIContent("Swap inverse3",
                    "Swap the two inverse3 controller's assigned devices")))
                {
                    _deviceMapper.SwapInverse3();
                    _waitingForVerseGripHandednessConfirm = true;
                }
            }

            // Enable or disable the cursor position update
            var probing= GUILayout.Toggle(controller.IsProbeCursorPosition, "Probe cursor position");
            if (controller.IsReady && probing != controller.IsProbeCursorPosition)
            {
                controller.ProbeCursorPosition(probing);
            }
        }
        private void VerseGripControllerGUI(VerseGripController controller)
        {
            // Before the device is assigned, we can select the VerseGrip type
            if (!controller.Assigned)
            {
                GUILayout.Label("VerseGripController \u2192 <not assigned>", GUILayout.Width(800));
                if (GUILayout.Button($"Filter: {controller.verseGripTypeFilter}"))
                {
                    switch (controller.verseGripTypeFilter)
                    {
                        case VerseGripType.Wired:
                            controller.verseGripTypeFilter = VerseGripType.Wireless;
                            break;
                        case VerseGripType.Wireless:
                            controller.verseGripTypeFilter = VerseGripType.Any;
                            break;
                        case VerseGripType.Any:
                            controller.verseGripTypeFilter = VerseGripType.Wired;
                            break;
                    }
                }
            }
            
            // Once the device is ready, devices can be swapped
            else
            {
                GUILayout.Label($"VerseGripController \u2192 #{controller.DeviceId}.{controller.VerseGripType}", GUILayout.Width(800));

                // Swap the two verse grip controllers' assigned devices
                if (GUILayout.Button("Swap verse grip"))
                {
                    _deviceMapper.SwapVerseGrip();
                    _waitingForVerseGripHandednessConfirm = true;
                }
            }
        }

        // Display the GUI
        private void OnGUI()
        {
            // Show the device mapper state and actions
            GUILayout.BeginArea(new Rect(10, 10, 400, 200), new GUIStyle(GUI.skin.box));
            DeviceMapperGUI();
            GUILayout.Space(10);
            // Show the device list
            DeviceListGUI();
            GUILayout.EndArea();

            // Show the left inverse3 controller
            var leftRect = new Rect(0, Screen.height - 200, 300, 200);
            leftRect.x = 10;
            GUILayout.BeginArea(leftRect, new GUIStyle(GUI.skin.box));
            GUILayout.Label(leftInverse3.gameObject.name, GUILayout.Width(600));

            Inverse3ControllerGUI(leftInverse3);

            GUILayout.Space(10);

            // Show the associated verse grip controller
            var leftVerseGripController = _deviceMapper.GetVerseGripController(leftInverse3);
            VerseGripControllerGUI(leftVerseGripController);
            GUILayout.EndArea();

            // Show the right controller
            var rightRect = new Rect(0, Screen.height - 200, 300, 200);
            rightRect.x = Screen.width - rightRect.width - 10;
            GUILayout.BeginArea(rightRect, new GUIStyle(GUI.skin.box));
            GUILayout.Label(rightInverse3.gameObject.name, GUILayout.Width(600));

            Inverse3ControllerGUI(rightInverse3);

            GUILayout.Space(10);

            // Show the associated verse grip controller
            var rightVerseGripController = _deviceMapper.GetVerseGripController(rightInverse3);
            VerseGripControllerGUI(rightVerseGripController);

            GUILayout.EndArea();
        }
    }
}
