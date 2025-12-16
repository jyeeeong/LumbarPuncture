using UnityEngine;

/// <summary>
/// Haptic force effector interface.
/// Implementers compute force only (NO SetCursorLocalForce here).
/// Called from the haptic thread.
/// </summary>
public interface IHapticEffector
{
    /// <param name="cursorLocalPos">Cursor position (Inverse3 local)</param>
    /// <param name="cursorLocalVel">Cursor velocity (Inverse3 local)</param>
    /// <param name="cursorRadius">Cursor tip radius</param>
    /// <returns>Force contribution in Inverse3 local coordinates</returns>
    Vector3 ComputeForce(
        in Vector3 cursorLocalPos,
        in Vector3 cursorLocalVel,
        float cursorRadius
    );
}
