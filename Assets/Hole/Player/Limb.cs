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

    // -- config --
    /// the name of the limb
    [SerializeField] Name mName;

    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    /// the shared prompt ui
    Prompt mPrompt;

    // -- props --
    /// the input action for this limb
    InputAction mInput;

    // -- lifecycle --
    void Awake() {
        // get node deps
        mPrompt = Prompt.Get;
        mInput = mInputs.FindAction(FindActionName());
    }

    void OnDisable() {
        mInput.Disable();
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
        mInput.Enable();
    }

    // -- queries --
    /// if the limb was just pressed
    public bool IsJustPressed() {
        return mInput.WasPerformedThisFrame();
    }

    /// if the limb is pressed
    public bool IsPressed() {
        return mInput.IsPressed();
    }

    /// if the limb was just released
    public bool IsJustReleased() {
        return mInput.WasReleasedThisFrame();
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