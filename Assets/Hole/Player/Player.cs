using Hertzole.GoldPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- statics --
    /// the wall layer
    int sWallLayer = -1;

    // -- nodes --
    /// the player's root transform
    Transform mRoot;

    /// the custom player input
    PlayerInput mInput;

    /// the player's physics body
    Rigidbody mBody;

    /// the player controller
    CharacterController mCharacter;

    /// the player controller
    GoldPlayerController mController;

    /// the player's viewpoint
    Transform mView;

    /// the player's left foot
    Limb mLeftFoot;

    /// the player's right foot
    Limb mRightFoot;

    /// the player's left hand
    Limb mLeftHand;

    /// the player's right hand
    Limb mRightHand;

    // -- inputs --
    /// the bind action
    InputAction mBind;

    // -- props --
    /// the limb being bound
    Limb mBinding;

    /// a shared buffer for raycast hits
    readonly RaycastHit[] mHits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        // get node dependencies
        var t = transform;
        mRoot = t;
        mInput = GetComponent<PlayerInput>();

        var tb = t.Find("Body");
        mBody = tb.GetComponent<Rigidbody>();
        mCharacter = tb.GetComponent<CharacterController>();
        mController = tb.GetComponent<GoldPlayerController>();

        mView = t.Find("Body/Head/View");
        mLeftHand = t.Find("Limbs/LeftHand").GetComponent<Limb>();
        mRightHand = t.Find("Limbs/RightHand").GetComponent<Limb>();
        mLeftFoot = t.Find("Limbs/LeftFoot").GetComponent<Limb>();
        mRightFoot = t.Find("Limbs/RightFoot").GetComponent<Limb>();

        // bind input events
        var inputs = GetComponentInChildren<GoldPlayerInputSystem>().InputAsset;
        mBind = inputs.FindAction("StartBind");
        mBind.performed += OnBind;

        // set statics
        if (sWallLayer == -1) {
            sWallLayer = LayerMask.NameToLayer("Wall");
        }
    }

    void OnEnable() {
        mBind.Enable();
    }

    void Start() {
        SetClimbing(false);
    }

    void Update() {
        // if we finished binding the current limb, bind the next one
        if (mBinding != null && mBinding.IsJustPressed()) {
            BindNextLimb();
        }

        // attach body to wall
        if (mLeftFoot.IsJustPressed()) {
            // cast for a wall
            var nHits = Physics.RaycastNonAlloc(
                mLeftFoot.position,
                mRoot.forward,
                mHits,
                1.0f,
                1 << sWallLayer
            );

            // bind the inputs if we hit
            if (nHits != 0) {
                SetClimbing(true);
                var hit = mHits[0];
                mLeftFoot.position = hit.point;
            }
        }
    }

    // -- commands --
    /// try to bind inputs
    void TryBind() {
        // cast for a wall
        var nHits = Physics.RaycastNonAlloc(
            mView.position,
            mView.forward,
            mHits,
            5.0f,
            1 << sWallLayer
        );

        // climb if it hits
        if (nHits != 0) {
            StartBinding();
        }
    }

    /// start binding inputs for each limb
    void StartBinding() {
        // disable bind input
        mBind.Disable();

        // begin binding limbs
        BindNextLimb();
    }

    void BindNextLimb() {
        // find a limb to bind, if any
        var limb = FindNextLimb();
        if (limb == null) {
            return;
        }

        // bind its input
        limb.Bind();
    }

    /// configure character for climbing or no
    void SetClimbing(bool isClimbing) {
        mCharacter.enabled = !isClimbing;
        mController.enabled = !isClimbing;
        mBody.isKinematic = !isClimbing;
        mBody.detectCollisions = isClimbing;
    }

    // -- queries --
    /// finds the next limb to bind
    Limb FindNextLimb() {
        var curr = mBinding;

        if (curr == null) {
            return mLeftFoot;
        } else if (curr == mLeftFoot) {
            return mRightFoot;
        } else if (curr == mRightFoot) {
            return mLeftHand;
        } else if (curr == mLeftHand) {
            return mRightHand;
        } else { // mRightHand
            return null;
        }
    }

    // -- events --
    void OnBind(InputAction.CallbackContext ctx) {
        TryBind();
    }
}
