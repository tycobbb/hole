using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera: MonoBehaviour {
    // -- config --
    /// the look limits in degrees
    [SerializeField] Vector2 mLimits = new Vector2(30.0f, 75.0f);

    /// the look sensitivity in degrees per unit
    [SerializeField] Vector2 mSensitivity = new Vector2(2.0f, 2.0f);

    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    // -- c/nodes
    /// the body's root transform
    [SerializeField] Transform mRoot;

    /// the body's viewpoint
    [SerializeField] Transform mView;

    // -- props --
    /// the look action
    InputAction mLook;

    /// the look direction
    Vector2 mLookDir;

    // -- lifecycle --
    void Awake() {
        // get props
        mLook = mInputs.FindAction("Look");

        // lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        // reduce sensitivity in webgl
        #if !UNITY_EDITOR && UNITY_WEBGL
        mLookSensitivity *= 0.25f;
        #endif
    }

    void OnEnable() {
        mLook.Enable();
    }

    void Update() {
        // add scaled input to look dir
        var dir = mLookDir + mLook.ReadValue<Vector2>() * mSensitivity;

        // apply rotation limits
        var dxm = mLimits.x;
        var dym = mLimits.y;
        dir.x = Mathf.Clamp(dir.x, -dxm, dxm);
        dir.y = Mathf.Clamp(dir.y, -dym, dym);

        // update look dir
        mLookDir = dir;

        // get rotations
        var xRot = Quaternion.AngleAxis(mLookDir.x, Vector3.up);
        var yRot = Quaternion.AngleAxis(mLookDir.y, Vector3.left);

        // apply to body / view
        mRoot.localRotation = xRot;
        mView.localRotation = yRot;
    }

    void OnDisable() {
        mLook.Disable();
    }
}