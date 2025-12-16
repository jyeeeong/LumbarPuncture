// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class HapticForceManager : MonoBehaviour
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
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

public class HapticForceManager : MonoBehaviour
{
    public Inverse3Controller inverse3;

    [Header("Safety")]
    public bool forceEnabled = true;

    [Tooltip("전체 힘 상한(안전용)")]
    public float maxForceMagnitude = 20f;

    private readonly List<IHapticEffector> _effectors = new();

    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();
    }

    private void OnEnable()
    {
        RegisterAllEffectorsInScene();
        inverse3.DeviceStateChanged += OnDeviceStateChanged;
    }

    private void OnDisable()
    {
        inverse3.DeviceStateChanged -= OnDeviceStateChanged;
        // inverse3.Release(); // 필요하면 유지. 샘플들처럼 쓰는 패턴이면 켜도 됨.
    }

    private void RegisterAllEffectorsInScene()
    {
        _effectors.Clear();
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IHapticEffector e) _effectors.Add(e);
        }
        Debug.Log($"[HapticForceManager] Registered effectors: {_effectors.Count}");
    }

    private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
    {
        var dev = args.DeviceController;

        if (!forceEnabled)
        {
            dev.SetCursorLocalForce(Vector3.zero);
            return;
        }

        Vector3 cursorPos = dev.CursorLocalPosition;
        Vector3 cursorVel = dev.CursorLocalVelocity;
        float cursorRadius = inverse3.Cursor.Radius;

        Vector3 total = Vector3.zero;

        // 합산
        for (int i = 0; i < _effectors.Count; i++)
            total += _effectors[i].ComputeForce(cursorPos, cursorVel, cursorRadius);

        // 안전 clamp
        float mag = total.magnitude;
        if (mag > maxForceMagnitude && mag > 1e-6f)
            total = total * (maxForceMagnitude / mag);

        dev.SetCursorLocalForce(total);
    }
}
