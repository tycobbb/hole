using System;
using System.Collections;
using Hertzole.GoldPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

/// the player's custom inputs
public class PlayerInput: MonoBehaviour {
    // -- nodes --
    /// all the player's inputs
    InputActionAsset mInputs;

    /// the ui prompt
    Prompt mPrompt;

    // -- inputs --
    /// the initial climb input
    InputAction mClimb;

    /// the left hand input
    InputAction mLeftHand;

    /// the right hand input
    InputAction mRightHand;

    /// the left foot input
    InputAction mLeftFoot;

    /// the right foot input
    InputAction mRightFoot;

    // -- props --
    /// if the custom input is active
    bool mActive = false;

    /// the current limb being bound
    Limb.Name? mBinding = null;

    /// an action to fire after binding a limb
    Action<Limb.Name> mOnBinding;

    // -- lifecycle --
    void Awake() {
        // get node dependencies
        mInputs = GetComponentInChildren<GoldPlayerInputSystem>().InputAsset;
        mPrompt = Prompt.Get;

        // get actions
        mClimb = mInputs.FindAction("Climb");
        mLeftHand = mInputs.FindAction("LeftHand");
        mRightHand = mInputs.FindAction("RightHand");
        mLeftFoot = mInputs.FindAction("LeftFoot");
        mRightFoot = mInputs.FindAction("RightFoot");
    }

    void OnEnable() {
        mClimb.Enable();
    }

    void Start() {
        StartCoroutine(Activate());
    }

    void OnDisable() {
        mInputs.Disable();
    }

    // -- commands --
    /// bind the next input
    public void BindNext() {
        // SetBinding(mBinding + 1);
    }

    /// add an action when starting to climb
    public void OnClimb(Action action) {
        mClimb.performed += (ctx) => {
            if (mActive) {
                action.Invoke();
            }
        };
    }

    /// add an action when binding an input
    // public void OnBind(Action<Binding> action) {
        // mOnBinding = action;
    // }

    /// wait a second to activate to avoid noisy initial inputs
    IEnumerator Activate() {
        yield return new WaitForSeconds(0.1f);
        mActive = true;
    }

    /// sets the binding w/ side effects
    // -- queries --
    public InputAction LeftHand {
        get => mLeftHand;
    }

    public InputAction RightHand {
        get => mRightHand;
    }

    public InputAction LeftFoot {
        get => mLeftFoot;
    }

    public InputAction RightFoot {
        get => mRightFoot;
    }
}
