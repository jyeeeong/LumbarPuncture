using UnityEngine;

public class DripController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem ps;
    public Transform droplet;

    [Header("State")]
    public bool dripping = false;

    [Header("Droplet visuals")]
    public float dropletMinScale = 0.002f;
    public float dropletMaxScale = 0.02f;
    public float growSpeed = 0.05f;
    public float resetShrinkSpeed = 0.2f;

    [Header("Timing randomness")]
    public float minHoldBeforeDrop = 0.05f;
    public float maxHoldBeforeDrop = 0.20f;

    [Header("Emit")]
    public int dropsPerEmit = 1;

    float holdTimer = 0f;
    bool waitingToDrop = false;
    bool started = false;

    void Awake()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (droplet != null) droplet.localScale = Vector3.one * dropletMinScale;
    }

    void EnsurePlaying()
    {
        if (ps == null) return;
        if (!started)
        {
            ps.Play(true);   // ✅ Play On Awake OFF여도 여기서 시작
            started = true;
        }
    }

    void Update()
    {
        if (!dripping)
        {
            if (droplet != null)
                droplet.localScale = Vector3.one * dropletMinScale;
            waitingToDrop = false;
            return;
        }

        if (ps == null || droplet == null) return;

        EnsurePlaying();

        if (!waitingToDrop)
        {
            float s = droplet.localScale.x;
            s += growSpeed * Time.deltaTime;
            s = Mathf.Min(s, dropletMaxScale);
            droplet.localScale = Vector3.one * s;

            if (s >= dropletMaxScale - 1e-6f)
            {
                waitingToDrop = true;
                holdTimer = Random.Range(minHoldBeforeDrop, maxHoldBeforeDrop);
            }
        }
        else
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                ps.Emit(dropsPerEmit);
                waitingToDrop = false;
            }

            float s = Mathf.MoveTowards(
                droplet.localScale.x,
                dropletMinScale,
                resetShrinkSpeed * Time.deltaTime
            );
            droplet.localScale = Vector3.one * s;
        }
    }

    public void StartDrip()
    {
        dripping = true;
        waitingToDrop = false;
        started = false; // 다음 Update에서 EnsurePlaying이 Play() 호출하게
    }

    public void StopDrip()
    {
        dripping = false;
        waitingToDrop = false;
        started = false;

        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (droplet != null)
            droplet.localScale = Vector3.one * dropletMinScale;
    }
}
