using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeManager : MonoBehaviour
{
    public enum AppMode { Normal = 2, OutlineOff = 1 }
    public AppMode currentMode = AppMode.Normal;

    [Header("UI")]
    public TextMeshProUGUI modeGuideText;

    private List<Outline> cachedOutlines = new List<Outline>();

    void Start()
    {
        cachedOutlines.Clear();  
        currentMode = AppMode.Normal;
        UpdateUIText();
    }
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SwitchMode(AppMode.OutlineOff);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SwitchMode(AppMode.Normal);
    }

    void SwitchMode(AppMode next)
    {
        if (currentMode == next) return;

        currentMode = next;

        switch (currentMode)
        {
            case AppMode.OutlineOff:
                DisableAllOutlines();
                break;

            case AppMode.Normal:
                RestoreAllOutlines();
                break;
        }

        UpdateUIText();
    }

    void DisableAllOutlines()
    {
        cachedOutlines.Clear();

        var outlines = Object.FindObjectsByType<Outline>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var o in outlines)
        {
            if (o.enabled)
            {
                cachedOutlines.Add(o);
                o.enabled = false;
            }
        }
    }

    void RestoreAllOutlines()
    {
        foreach (var o in cachedOutlines)
        {
            if (o != null)
                o.enabled = true;
        }
    }

    void UpdateUIText()
    {
        if (modeGuideText == null) return;

        switch (currentMode)
        {
            case AppMode.OutlineOff:
                modeGuideText.text =
                    "<b>[Focus mode]</b>\n" +
                    "Press <b>2</b> to show outlines";
                break;

            case AppMode.Normal:
                modeGuideText.text =
                    "<b>[Guide mode]</b>\n" +
                    "Press <b>1</b> to hide outlines";
                break;
        }
    }

}
