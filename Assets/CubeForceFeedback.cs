// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class CubeForceFeedback : MonoBehaviour
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

using UnityEngine;
using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;

public class CubeForceFeedback : MonoBehaviour
{
    public Inverse3Controller inverse3;

    [Range(0, 800)] public float stiffness = 300f;
    [Range(0, 3)]   public float damping = 1f;

    public float cursorRadius = 0.002f; // 바늘 끝 radius

    private Vector3 cubeCenterLocal;
    private Vector3 halfSize;        // cube extents in local space

    private void Awake()
    {
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();

        inverse3.Ready.AddListener((d, a) =>
        {
            // cube의 월드 → Inverse3 로컬 변환
            cubeCenterLocal = inverse3.transform.InverseTransformPoint(transform.position);

            // 큐브 사이즈 계산 (localScale 사용)
            Vector3 s = transform.lossyScale;
            halfSize = s * 0.5f;
        });
    }

    private void OnEnable()
    {
        inverse3.DeviceStateChanged += OnDeviceStateChanged;
    }

    private void OnDisable()
    {
        inverse3.DeviceStateChanged -= OnDeviceStateChanged;
    }

    private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
    {
        var dev = args.DeviceController;

        Vector3 cursorLocal    = dev.CursorLocalPosition;
        Vector3 cursorVelLocal = dev.CursorLocalVelocity;

        // 1) 큐브 내부에서 가장 가까운 표면점 계산
        Vector3 diff = cursorLocal - cubeCenterLocal;

        Vector3 clamped = new Vector3(
            Mathf.Clamp(diff.x, -halfSize.x, halfSize.x),
            Mathf.Clamp(diff.y, -halfSize.y, halfSize.y),
            Mathf.Clamp(diff.z, -halfSize.z, halfSize.z)
        );

        Vector3 closestPoint = cubeCenterLocal + clamped;

        // 2) 침투량 계산
        Vector3 distanceVec = cursorLocal - closestPoint;
        float distance = distanceVec.magnitude;
        float penetration = cursorRadius - distance;

        // 3) 힘 적용
        if (penetration > 0)
        {
            Vector3 normal = distanceVec.normalized;
            Vector3 force = normal * penetration * stiffness;
            force -= cursorVelLocal * damping;

            dev.SetCursorLocalForce(force);
        }
        else
        {
            dev.SetCursorLocalForce(Vector3.zero);
        }
    }
}
