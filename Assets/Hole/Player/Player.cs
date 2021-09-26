using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- constants --
    /// the value when no limb is selected
    const int kLimbNone = -1;

    /// the wall layer
    static int sWallLayer = -1;

    /// the floor layer
    static int sFloorLayer = -1;

    // -- types --
    /// the player's current state
    enum State {
        Grounded,
        Climbing,
        Falling,
    }

    // -- deps --
    /// the shared score ui
    Score mScore;

    /// the shared prompt ui
    Prompt mPrompt;

    // -- config --
    /// the speed the rotation animates
    [SerializeField] float mRotateSpeed = 2.0f;

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
    /// the current player state
    State mState = State.Grounded;

    /// the rotation to animate to, if any
    Quaternion? mRotation = null;

    /// the limb being bound
    int mCurrentLimb = kLimbNone;

    /// the bind limbs action
    InputAction mBindLimbs;

    /// a shared buffer for raycast hits
    readonly RaycastHit[] mHits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        // get deps
        mScore = Score.Get;
        mPrompt = Prompt.Get;

        // bind input events
        mBindLimbs = mInputs.FindAction("BindLimbs");
        mBindLimbs.performed += OnBindLimbs;

        // set statics
        if (sWallLayer == -1) {
            sWallLayer = LayerMask.NameToLayer("Wall");
            sFloorLayer = LayerMask.NameToLayer("Floor");
        }
    }

    void OnEnable() {
        mBindLimbs.Enable();
    }

    void Start() {
        SetState(mState);
    }

    void Update() {
        // if we bound the current limb, attach it
        var curr = FindCurrentLimb();
        if (curr != null && curr.IsJustPressed()) {
            TryFinishBind();
        }

        // update every limb
        var nContacts = 0;
        foreach (var limb in mLimbs) {
            // count limbs grabbing
            var isGrabbing = !IsFalling && limb.IsPressed();
            if (isGrabbing) {
                nContacts += 1;
            }

            // pin it in place when grabbing
            limb.IsPinned = isGrabbing;

            // when just pressed try to grab/climb
            if (limb.IsJustPressed()) {
                if (IsGrounded) {
                    TryGrab(limb);
                } else {
                    TryClimb(limb);
                }
            }

            // rotate smoothly
            if (mRotation != null) {
                mRoot.rotation = Quaternion.Lerp(
                    mRoot.rotation,
                    mRotation.Value,
                    Time.deltaTime * mRotateSpeed
                );
            }
        }

        // start climbing once every limb is on the wall
        if (ShouldStartClimb(nContacts)) {
            StartClimb();
        }

        // fall if more than one limb is off the wall
        if (ShouldFall(nContacts)) {
            Fall();
        }

        if (ShouldLand()) {
            Land();
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

        // if the next limb exists
        var limb = FindCurrentLimb();
        if (limb == null) {
            return;
        }

        // excluding any key already used
        var exclusions = new string[mCurrentLimb];
        for (var i = 0; i < mCurrentLimb; i++) {
            exclusions[i] = mLimbs[i].FindControlPath();
        }

        // bind its input
        limb.Bind(exclusions);
    }

    /// grab the wall with the current limb and bind the next one
    void TryFinishBind() {
        var limb = FindCurrentLimb();

        // try to grab the wall
        var attached = TryGrab(limb);
        if (!attached) {
            return;
        }

        // finish binding this limb and move on to the next one
        BindNextLimb();
    }

    /// try to grab a wall with the limb
    bool TryGrab(Limb limb) {
        // find the contact point
        var hit = FindWallHit(limb);
        if (hit == null) {
            return false;
        }

        // attach to the wall
        MoveLimb(limb, hit.Value.point);

        return true;
    }

    /// climb a little higher up with the limb
    void TryClimb(Limb limb) {
        // find the contact point
        var hit = FindWallHit(limb);
        if (hit == null) {
            return;
        }

        // attach to the wall, but a little higher
        var p = hit.Value.point;
        p.y += 1.0f;
        MoveLimb(limb, p);
    }

    /// move limb to point
    void MoveLimb(Limb limb, Vector3 pos) {
        limb.MoveTo(pos);

        // record the score
        mScore.Record((int)(pos.y * 10.0f));
    }

    /// start climbing
    void StartClimb() {
        SetState(State.Climbing);

        // hide any instructional text
        mPrompt.Hide();
    }

    /// start falling
    void Fall() {
        mRotation = Quaternion.AngleAxis(-90.0f, Vector3.right);
        SetState(State.Falling);
    }

    /// land on the ground after falling
    void Land() {
        mRotation = Quaternion.identity;
        SetState(State.Grounded);
    }

    /// configure character for climbing or not
    void SetState(State state) {
        mState = state;

        // ignore physics while binding
        mBody.isKinematic = state == State.Grounded;
    }

    // -- queries --
    /// if the player is idle and ready to grab
    bool IsGrounded {
        get => mState == State.Grounded;
    }

    /// if the player is on the wall and climbing
    bool IsClimbing {
        get => mState == State.Climbing;
    }

    /// if the player is falling
    bool IsFalling {
        get => mState == State.Falling;
    }

    /// find the current limb, if any
    Limb FindCurrentLimb() {
        if (mCurrentLimb == kLimbNone || mCurrentLimb >= mLimbs.Length) {
            return null;
        }

        return mLimbs[mCurrentLimb];
    }

    /// casts for a wall hit from the limb
    RaycastHit? FindWallHit(Limb limb) {
        // cast for a wall
        var nHits = Physics.RaycastNonAlloc(
            limb.Position,
            mRoot.forward,
            mHits,
            5.0f,
            1 << sWallLayer
        );

        if (nHits == 0) {
            return null;
        }

        return mHits[0];
    }

    /// if the player should start a climb
    bool ShouldStartClimb(int nContacts) {
        // climb once all limbs are in contact
        return IsGrounded && nContacts == mLimbs.Length;
    }

    /// if the player should fall from a climb
    bool ShouldFall(int nContacts) {
        // fall if more than one contact is missing
        return IsClimbing && nContacts < mLimbs.Length - 1;
    }

    /// if the player should land this frame
    bool ShouldLand() {
        // make sure the player is falling
        if (!IsFalling) {
            return false;
        }

        // cast downwards for the floor
        var nHits = Physics.RaycastNonAlloc(
            mRoot.position,
            Vector3.down,
            mHits,
            1.0f,
            1 << sFloorLayer
        );

        return nHits != 0;
    }

    // -- events --
    void OnBindLimbs(InputAction.CallbackContext ctx) {
        TryBindLimbs();
    }
}