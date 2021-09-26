using UnityEngine;
using UnityEngine.InputSystem;

/// one of the player's limbs
public class Limb: MonoBehaviour {
    // -- types --
    /// the type of limb
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
    /// the limb's root transform
    [SerializeField] Transform mRoot;

    /// the limb's rigidbody
    [SerializeField] Rigidbody mBody;

    // -- props --
    /// the position to animate to smoothly
    Vector3 mPosition;

    /// if the limb is pinned externally
    bool mIsPinned;

    /// the grab action for this limb
    InputAction mGrab;

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
        mBody.isKinematic = IsPinned;
    }

    void FixedUpdate() {
        if (IsPinned) {
            mRoot.position = mPosition;
        }
    }

    void OnDisable() {
        mGrab.Disable();
    }

    // -- commands --
    /// show prompt and bind this limb's input
    public void Bind(string[] exclusions = null) {
        // show the prompt
        mPrompt.Show(FindPromptText());

        // bind the grab to the next key held for 0.3s
        var rebind = mGrab
            .PerformInteractiveRebinding()
            .WithRebindAddingNewBinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Keyboard/anyKey")
            .OnMatchWaitForAnother(0.1f);

        // exclude any extra paths
        if (exclusions != null) {
            foreach (var path in exclusions) {
                rebind = rebind.WithControlsExcluding(path);
            }
        }

        // start the rebind
        rebind
            .Start()
            .OnComplete((op) => {
                op.Dispose();
                mGrab.Enable();
            });
    }

    /// moves the limb to the position smoothly
    public void MoveTo(Vector3 pos) {
        mPosition = pos;
    }

    // -- props(hot) --
    /// if it's pinned in place
    public bool IsPinned {
        get => mIsPinned;
        set => mIsPinned = value;
    }

    // -- queries --
    /// its transform position
    public Vector3 Position {
        get => mRoot.position;
    }

    /// if the limb was just pressed
    public bool IsJustPressed() {
        return mGrab.WasPerformedThisFrame();
    }

    /// if the limb is pressed
    public bool IsPressed() {
        // if the action is pressed
        if (mGrab.IsPressed()) {
            return true;
        }

        // or the control is pressed (feel like we shouldn't need this but
        // initial input binding breaks otherwise
        var control = FindControl();
        if (control != null && control.IsPressed()) {
            return true;
        }

        return false;
    }

    /// finds the grab control path, if any
    public string FindControlPath() {
        if (!mGrab.enabled) {
            return null;
        }

        return FindControl()?.path;
    }

    /// finds the current grab control, if any
    InputControl FindControl() {
        if (mGrab.controls.Count == 0) {
            return null;
        }

        return mGrab.controls[0];
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
            Name.LeftHand => "tap & hold 3 w/ ur ring finger",
            Name.RightHand => "tap & hold the farthest key u can w/ ur index finger",
            Name.LeftFoot => "tap & hold the farthest key u can w/ ur left pinky",
            Name.RightFoot => "tap & hold the farthest key u can w/ ur thumb",
            _ => null,
        };
    }
}