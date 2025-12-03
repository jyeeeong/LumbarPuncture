using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AngleDisplay : MonoBehaviour
{
    public Transform plane;
    public Transform hapticOrigin;
    public TMP_Text angleText;
    public Image circleImage;

    void Update()
    {
        // planeNormal = plane.up (또는 plane.right 등 진짜 법선)
        Vector3 planeNormal = plane.up;
        Vector3 rayDir = hapticOrigin.forward;

        // 평면에 투영
        Vector3 rayOnPlane = rayDir - Vector3.Dot(rayDir, planeNormal) * planeNormal;
        rayOnPlane.Normalize();

        // 선–면 각도 (0~90)
        float linePlaneAngle = Vector3.Angle(rayDir, rayOnPlane);

        // UI 표시
        angleText.text = $"Angle: {linePlaneAngle:F1}°";

        // 색상 변경
        if (linePlaneAngle > 80f && linePlaneAngle <= 90f)
            circleImage.color = Color.green;
        else if (linePlaneAngle >= 75f && linePlaneAngle <= 80f)
            circleImage.color = Color.yellow;
        else
            circleImage.color = Color.red;
    }
}
