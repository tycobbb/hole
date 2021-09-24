using Hertzole.GoldPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

/// the player
public class Player: MonoBehaviour {
    // -- statics --
    /// the wall layer
    int sWallLayer = -1;

    // -- config --
    /// the input asset
    [SerializeField] InputActionAsset mInputs;

    // -- nodes --
    /// the player's root transform
    Transform mRoot;

    /// the player's viewpoint
    Transform mView;

    /// the player's physics body
    Rigidbody mBody;

    /// the player controller
    CharacterController mCharacter;

    /// the player controller
    GoldPlayerController mController;

    /// the player's left foot
    Limb mLeftFoot;

    /// the player's right foot
    Limb mRightFoot;

    /// the player's left hand
    Limb mLeftHand;

    /// the player's right hand
    Limb mRightHand;

    // -- inputs --
    /// the bind limbs action
    InputAction mBindLimbs;

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
        mView = t.Find("Body/Head/View");

        // get body
        var tb = t.Find("Body");
        mBody = tb.GetComponent<Rigidbody>();
        mCharacter = tb.GetComponent<CharacterController>();
        mController = tb.GetComponent<GoldPlayerController>();

        // get limbs
        mLeftHand = t.Find("Limbs/LeftHand").GetComponent<Limb>();
        mRightHand = t.Find("Limbs/RightHand").GetComponent<Limb>();
        mLeftFoot = t.Find("Limbs/LeftFoot").GetComponent<Limb>();
        mRightFoot = t.Find("Limbs/RightFoot").GetComponent<Limb>();

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
    void OnBindLimbs(InputAction.CallbackContext ctx) {
        TryBindLimbs();
    }
}