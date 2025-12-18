using System.Collections.Generic;
using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public enum AppMode { Normal = 1, OutlineOff = 2 }
    public AppMode currentMode = AppMode.Normal;

    // 꺼두었던 Outline들을 저장
    private List<Outline> cachedOutlines = new List<Outline>();

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
    }

    /// <summary>
    /// 현재 활성화된 Outline만 끄고, 목록에 저장
    /// </summary>
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
                o.enabled = false; // OnDisable → 머티리얼 제거
            }
        }

        Debug.Log($"[ModeManager] Disabled {cachedOutlines.Count} outlines");
    }

    /// <summary>
    /// 이전에 꺼두었던 Outline만 다시 켜기
    /// </summary>
    void RestoreAllOutlines()
    {
        foreach (var o in cachedOutlines)
        {
            if (o != null)
                o.enabled = true; // OnEnable → 머티리얼 복구
        }

        Debug.Log($"[ModeManager] Restored {cachedOutlines.Count} outlines");
    }
}
