using System;
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

    // -- c/nodes
    /// the limb's rigidbody
    [SerializeField] Rigidbody mBody;

    // -- props --
    /// if the grab has been used
    bool mHasGrabbed;

    /// the grab action for this limb
    InputAction mGrab;

    // -- lifecycle --
    void Awake() {
        // get deps
        mPrompt = Prompt.Get;

        // set props
        mGrab = mInputs.FindAction(FindActionName());
    }

    void Update() {
        // track first grab
        if (!mHasGrabbed && mGrab.WasPressedThisFrame()) {
            mHasGrabbed = true;
        }

        // keep the limb pinned if unbound or unpressed
        mBody.isKinematic = IsPinned();
    }

    void OnDisable() {
        mGrab.Disable();
    }

    // -- props(hot) --
    /// the limb's transform position
    public Vector3 Position {
        get => transform.position;
        set => transform.position = value;
    }

    // -- commands --
    /// show prompt and bind this limb's input
    public void Bind() {
        mPrompt.Show(FindPromptText());

        // enable the grab
        mGrab.Enable();
    }

    // -- queries --
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