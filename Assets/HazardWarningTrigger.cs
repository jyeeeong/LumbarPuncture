using UnityEngine;

public class HazardWarningTrigger : MonoBehaviour
{
    public WarningUI ui;
    public string hazardTag = "Hazard";

    [TextArea]
    public string warningMessage = "Warning: Contact with bone";

    int contactCount = 0;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(hazardTag)) return;

        contactCount++;
        ui?.Show(warningMessage);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(hazardTag)) return;

        contactCount = Mathf.Max(0, contactCount - 1);
        if (contactCount == 0)
            ui?.HideSoon();
    }
}
