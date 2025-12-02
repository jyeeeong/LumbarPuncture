using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;
using UnityEngine;

public class BasicForceOnly : MonoBehaviour
{
    public Inverse3Controller inverse3;

    [Range(0, 800)]
    public float stiffness = 300f;

    [Range(0, 3)]
    public float damping = 1f;

    private float cursorRadius = 0.002f;
    private float surfaceRadius = 0.02f;


    private void OnEnable()
    {
        Debug.Log("Force script ENABLED");
        inverse3.DeviceStateChanged += OnDeviceStateChanged;
    }

    private void Awake()
    {
        Debug.Log("Awake called");
        inverse3 ??= FindFirstObjectByType<Inverse3Controller>();

        inverse3.Ready.AddListener((dev, args) =>
        {
            Debug.Log("Inverse3 READY fired");
        });
    }


    private void OnDeviceStateChanged(object sender, Inverse3EventArgs args)
    {
        var device = args.DeviceController;

        // 로컬 좌표계에서 커서 위치와 속도 가져오기
        Vector3 cursorLocal = device.CursorLocalPosition;
        Vector3 cursorVelLocal = device.CursorLocalVelocity;

        // 임시: 중심(0,0,0)에 있는 구 표면 기준 force (일단 힘만)
        Vector3 distanceVec = cursorLocal - Vector3.zero;
        float distance = distanceVec.magnitude;

        float penetration = (surfaceRadius + cursorRadius) - distance;

        if (penetration > 0f)
        {
            Vector3 normal = distanceVec.normalized;

            Vector3 force = normal * penetration * stiffness;
            force -= cursorVelLocal * damping;

            device.SetCursorLocalForce(force);
        }
        else
        {
            device.SetCursorLocalForce(Vector3.zero);
        }
    }
}
