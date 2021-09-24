using System;
using System.Linq;
using Hertzole.GoldPlayer;
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

    /// the gold player controller
    [SerializeField] GoldPlayerController mGpc;

    /// the player's limbs in binding order
    [SerializeField] Limb[] mLimbs;

    // -- props --
    /// if the player is currently climbing
    bool mIsClimbing = false;

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
            AttachCurrentLimb();
        }

        // update every limb
        var nLimbsAttached = 0;
        foreach (var limb in mLimbs) {
            if (limb.IsPressed()) {
                nLimbsAttached += 1;
            }
        }

        // start climbing once every limb is on the wall
        if (!mIsClimbing && nLimbsAttached == mLimbs.Length) {
            Climb();
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

    /// attach the current limb to the wall
    void AttachCurrentLimb() {
        var limb = FindCurrentLimb();

        // cast for a wall
        var nHits = Physics.RaycastNonAlloc(
            limb.Position,
            mRoot.forward,
            mHits,
            2.0f,
            1 << sWallLayer
        );

        if (nHits == 0) {
            return;
        }

        // attach to the wall
        limb.Position = mHits[0].point;

        // bind the next limb
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

    /// start climbing
    void Climb() {
        SetClimbing(true);

        // hide and instructional text
        mPrompt.Hide();
    }

    /// configure character for climbing or no
    void SetClimbing(bool isClimbing) {
        mIsClimbing = isClimbing;

        // update components
        mGpc.enabled = !isClimbing;
        mGpc.Controller.enabled = !isClimbing;
        mBody.isKinematic = !isClimbing;
        mBody.detectCollisions = isClimbing;
    }

    // -- queries --
    /// find the current limb, if any
    Limb FindCurrentLimb() {
        if (mCurrentLimb == kLimbNone || mCurrentLimb >= mLimbs.Length) {
            return null;
        }

        return mLimbs[mCurrentLimb];
    }

    // -- events --
    void OnBindLimbs(InputAction.CallbackContext ctx) {
        TryBindLimbs();
    }
}