using UnityEngine;

public static class ComponentExt {
    /// if this object is active
    public static bool IsActive(this Component c) {
        return c.gameObject.activeSelf;
    }

    /// set this object's active state
    public static void SetActive(this Component c, bool isActive) {
        c.gameObject.SetActive(isActive);
    }
}
