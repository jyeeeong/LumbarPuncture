using UnityEngine;
using TMPro;

public class AngleDisplay : MonoBehaviour
{
    public Transform plane;            // Plane 객체
    public Transform hapticOrigin;     // Ray를 쏘는 객체
    public TMP_Text angleText;         // UI Text(TMP)

    void Update()
    {
        // 1. Plane의 법선 벡터
        Vector3 planeNormal = plane.up;

        // 2. HapticOrigin의 forward(Z축)
        Vector3 rayDir = hapticOrigin.forward;

        // 3. 각도 계산
        float angle = Vector3.Angle(planeNormal, rayDir);

        // 4. UI에 출력
        angleText.text = $"Angle: {angle:F1}°";
    }
}
