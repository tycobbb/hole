using UnityEngine;
using UnityEngine.InputSystem;

/// one of the player's limbs
public class Limb: MonoBehaviour {
    // -- types --
    enum Name {
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand,
    }

    // -- deps --
    /// the shared prompt ui
    Prompt mPrompt;

    // -- config --
    /// the name of the limb
    [SerializeField] Name mName;

    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    /// the speed it animates into position
    [SerializeField] float mAnimSpeed = 2.0f;

    // -- c/nodes
    /// the limb's root transform
    [SerializeField] Transform mRoot;

    /// the limb's rigidbody
    [SerializeField] Rigidbody mBody;

    // -- props --
    /// if the grab has been used
    bool mHasGrabbed;

    /// the grab action for this limb
    InputAction mGrab;

    /// the position to animate to smoothly
    Vector3 mPosition;

    // -- lifecycle --
    void Awake() {
        // get deps
        mPrompt = Prompt.Get;

        // set props
        mGrab = mInputs.FindAction(FindActionName());
    }

    void Start() {
        // store current position
        mPosition = mRoot.position;
    }

    void Update() {
        // track first grab
        if (!mHasGrabbed && mGrab.WasPressedThisFrame()) {
            mHasGrabbed = true;
        }

        // keep the limb pinned if unbound or unpressed
        mBody.isKinematic = IsPinned();

        // move smoothly
        mRoot.position = Vector3.Lerp(
            mRoot.position,
            mPosition,
            Time.deltaTime * mAnimSpeed
        );
    }

    void OnDisable() {
        mGrab.Disable();
    }

    // -- commands --
    /// show prompt and bind this limb's input
    public void Bind() {
        mPrompt.Show(FindPromptText());

        // enable the grab
        mGrab.Enable();
    }

    /// snaps the limb to the position immediately
    public void SnapTo(Vector3 pos) {
        MoveTo(pos);
        mRoot.position = pos;
    }

    /// moves the limb to the position smoothly
    public void MoveTo(Vector3 pos) {
        mPosition = pos;
    }

    // -- queries --
    /// the limb's transform position
    public Vector3 Position {
        get => mRoot.position;
    }

    /// if the limb was just pressed
    public bool IsJustPressed() {
        return mGrab.WasPerformedThisFrame();
    }

    /// if the limb is pressed
    public bool IsPressed() {
        return mGrab.IsPressed();
    }

    /// if the limb was just released
    public bool IsJustReleased() {
        return mGrab.WasReleasedThisFrame();
    }

    /// if it's pinned in place
    bool IsPinned() {
        return !mHasGrabbed || mGrab.IsPressed();
    }

    /// find action name for this limb
    string FindActionName() {
        return (mName) switch {
            Name.LeftHand => "LeftHand",
            Name.RightHand => "RightHand",
            Name.LeftFoot => "LeftFoot",
            Name.RightFoot => "RightFoot",
            _ => null,
        };
    }

    /// find the prompt text for this limb
    string FindPromptText() {
        return (mName) switch {
            Name.LeftHand => "hold 3 w/ ur ring finger",
            Name.RightHand => "hold the farthest key u can w/ ur index finger",
            Name.LeftFoot => "hold the farthest key u can w/ ur left pinky",
            Name.RightFoot => "hold the farthest key u can w/ ur thumb",
            _ => null,
        };
    }
}