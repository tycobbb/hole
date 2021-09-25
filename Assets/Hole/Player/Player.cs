using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- constants --
    /// the value when no limb is selected
    const int kLimbNone = -1;

    /// the wall layer
    static int sWallLayer = -1;

    // -- deps --
    /// the shared prompt ui
    Prompt mPrompt;

    // -- config --
    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    // -- c/nodes
    /// the player's root transform
    [SerializeField] Transform mRoot;

    /// the player's viewpoint
    [SerializeField] Transform mView;

    /// the player's physics body
    [SerializeField] Rigidbody mBody;

    /// the player's limbs in binding order
    [SerializeField] Limb[] mLimbs;

    // -- props --
    /// if the player is currently climbing
    bool mIsClimbing;

    /// the limb being bound
    int mCurrentLimb = kLimbNone;

    /// the bind limbs action
    InputAction mBindLimbs;

    /// a shared buffer for raycast hits
    readonly RaycastHit[] mHits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        // get deps
        mPrompt = Prompt.Get;

        // bind input events
        mBindLimbs = mInputs.FindAction("BindLimbs");
        mBindLimbs.performed += OnBindLimbs;

        // set statics
        if (sWallLayer == -1) {
            sWallLayer = LayerMask.NameToLayer("Wall");
        }
    }

    void OnEnable() {
        mBindLimbs.Enable();
    }

    void Start() {
        SetClimbing(false);
    }

    void Update() {
        // if we bound the current limb, attach it
        var curr = FindCurrentLimb();
        if (curr != null && curr.IsJustPressed()) {
            FinishBind();
        }

        // update every limb
        var nContacts = 0;
        foreach (var limb in mLimbs) {
            if (limb.IsJustPressed()) {
                TryGrab(limb);
            }

            if (limb.IsPressed()) {
                nContacts += 1;
            }
        }

        // start climbing once every limb is on the wall
        if (ShouldClimb(nContacts)) {
            Climb();
        }

        // fall if more than one limb is off the wall
        if (ShouldFall(nContacts)) {
            Fall();
        }
    }

    // -- commands --
    /// try to bind inputs
    void TryBindLimbs() {
        // cast for a wall
        var nHits = Physics.RaycastNonAlloc(
            mView.position,
            mView.forward,
            mHits,
            5.0f,
            1 << sWallLayer
        );

        // bind limbs if it hits
        if (nHits != 0) {
            BindLimbs();
        }
    }

    /// start binding inputs for each limb
    void BindLimbs() {
        // disable bind input
        mBindLimbs.Disable();

        // begin binding limbs
        BindNextLimb();
    }

    /// binds the next limb in the sequence
    void BindNextLimb() {
        // update the index of the current limb
        var next = mCurrentLimb + 1;
        if (next >= mLimbs.Length) {
            next = kLimbNone;
        }

        mCurrentLimb = next;

        // and bind it, if it exists
        var limb = FindCurrentLimb();
        if (limb != null) {
            limb.Bind();
        }
    }

    /// grab the wall with the current limb and bind the next one
    void FinishBind() {
        var limb = FindCurrentLimb();

        // grab the wall
        var attached = TryGrab(limb);

        // bind the next limb if successful
        if (attached) {
            BindNextLimb();
        }
    }

    /// try to grab a wall with the limb
    bool TryGrab(Limb limb) {
        // cast for a wall
        var nHits = Physics.RaycastNonAlloc(
            limb.Position,
            mRoot.forward,
            mHits,
            5.0f,
            1 << sWallLayer
        );

        if (nHits == 0) {
            return false;
        }

        // attach to the wall
        limb.Position = mHits[0].point;

        return true;
    }

    /// start climbing
    void Climb() {
        SetClimbing(true);

        // hide any instructional text
        mPrompt.Hide();
    }

    /// start falling
    void Fall() {
        SetClimbing(false);
    }

    /// configure character for climbing or not
    void SetClimbing(bool isClimbing) {
        mIsClimbing = isClimbing;

        // ignore physics unless climbing
        mBody.isKinematic = !isClimbing;
    }

    // -- queries --
    /// find the current limb, if any
    Limb FindCurrentLimb() {
        if (mCurrentLimb == kLimbNone || mCurrentLimb >= mLimbs.Length) {
            return null;
        }

        return mLimbs[mCurrentLimb];
    }

    /// if the player should start a climb
    bool ShouldClimb(int nContacts) {
        // climb once all limbs are in contact
        return !mIsClimbing && nContacts == mLimbs.Length;
    }

    /// if the player should fall from a climb
    bool ShouldFall(int nContacts) {
        // fall if more than one contact is missing
        return mIsClimbing && nContacts < mLimbs.Length - 1;
    }

    // -- events --
    void OnBindLimbs(InputAction.CallbackContext ctx) {
        TryBindLimbs();
    }
}