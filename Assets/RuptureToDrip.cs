using UnityEngine;

public class RuptureToDrip : MonoBehaviour
{
    public PlaneLoREffector lor;     // ligament plane effector
    public DripController drip;      // your drip controller

    [Header("Condition")]
    public float requiredHoldSeconds = 3f;

    [Header("Behavior")]
    public bool stopDripWhenNotRuptured = false; // 필요하면 true로

    float hold;
    bool dripStarted;

    void Reset()
    {
        lor = FindFirstObjectByType<PlaneLoREffector>();
        drip = FindFirstObjectByType<DripController>();
    }

    void Update()
    {
        if (lor == null || drip == null) return;

        if (lor.IsRuptured)
        {
            hold += Time.deltaTime;

            if (!dripStarted && hold >= requiredHoldSeconds)
            {
                drip.StartDrip();
                dripStarted = true;
            }
        }
        else
        {
            // 연속 유지 조건이므로 false 되면 타이머 리셋
            hold = 0f;

            if (stopDripWhenNotRuptured && dripStarted)
            {
                drip.StopDrip();
                dripStarted = false;
            }
        }
    }
}
