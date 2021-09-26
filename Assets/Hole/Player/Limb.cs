using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

    /// the speed it animates into position
    [SerializeField] float mMoveSpeed = 2.0f;

    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    /// the spring joint
    [SerializeField] SpringJoint mSpring;

    /// the fixed joint
    [SerializeField] FixedJoint mFixed;

    // -- c/nodes
    /// the limb's root transform
    [SerializeField] Transform mRoot;

    /// the limb's rigidbody
    [SerializeField] Rigidbody mBody;

    // -- props --
    /// the position to animate to smoothly
    Vector3 mPosition;

    /// if the grab has been used
    bool mHasGrabbed;

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
        // track first grab
        if (!mHasGrabbed && mGrab.WasPressedThisFrame()) {
            mHasGrabbed = true;
        }

        // check if pinned
        var isPinned = IsPinned;

        // if so, make the body kinematic
        mBody.isKinematic = isPinned;

        // and animate the position
        if (isPinned) {
            mRoot.position = Vector3.Lerp(
                mRoot.position,
                mPosition,
                Time.deltaTime * mMoveSpeed
            );
        }
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

    /// moves the limb to the position smoothly
    public void MoveTo(Vector3 pos) {
        mPosition = pos;
    }

    // -- props(hot) --
    /// if it's pinned in place
    public bool IsPinned {
        get => !mHasGrabbed || mIsPinned;
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
        return mGrab.IsPressed();
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