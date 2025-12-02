using UnityEngine;
using Haply.Inverse.DeviceControllers;

public class HG_BodyVirtualWall : MonoBehaviour
{
    public Inverse3Controller inverse3;
    public Transform cursor;
    public Transform virtualWall;
    public Vector3 resetPosition;

    public float wallStiffness = 300f;
    public float damping = 1f;
    public float maxPenetration = 0.002f;

    private Vector3 wallPointWorld;
    private Vector3 wallNormalWorld;
    private bool touching = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("touch")) return;

        touching = true;

        // ★ 최초 접촉 지점
        wallPointWorld = other.ClosestPoint(cursor.position);

        // ★ 벽 법선(바늘이 찌르는 방향의 반대)
        wallNormalWorld = (wallPointWorld - cursor.position).normalized;

        // ★ 화면용 벽 생성 위치 & 방향
        virtualWall.position = wallPointWorld;
        virtualWall.rotation = Quaternion.LookRotation(wallNormalWorld);

        Debug.Log("[ENTER] 벽 생성 위치=" + wallPointWorld + "  normal=" + wallNormalWorld);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!touching) return;

        ApplyForceWall();

        // ★ 벽은 고정 → 업데이트 금지
        virtualWall.position = wallPointWorld;
        virtualWall.rotation = Quaternion.LookRotation(wallNormalWorld);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains("touch")) return;

        touching = false;
        virtualWall.position = resetPosition;

        Debug.Log("[EXIT] 벽 초기화");
    }

    private void ApplyForceWall()
    {
        Vector3 cursorLocal = inverse3.CursorLocalPosition;
        Vector3 cursorVelocityLocal = inverse3.CursorLocalVelocity;
        Vector3 wallLocal = inverse3.transform.InverseTransformPoint(wallPointWorld);

        float distance = Vector3.Distance(cursorLocal, wallLocal);
        float penetration = maxPenetration - distance;

        if (penetration > 0f)
        {
            // ★ 법선을 로컬공간으로 변환
            Vector3 normalLocal = inverse3.transform.InverseTransformDirection(wallNormalWorld);

            Vector3 force = normalLocal * penetration * wallStiffness;
            force -= cursorVelocityLocal * damping;

            inverse3.SetCursorLocalForce(force);
        }
        else
        {
            inverse3.SetCursorLocalForce(Vector3.zero);
        }
    }
}
