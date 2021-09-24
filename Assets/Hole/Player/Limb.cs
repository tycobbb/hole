using System;
using Hertzole.GoldPlayer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// one of the player's limbs
public class Limb: MonoBehaviour {
    // -- types --
    public enum Name {
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand,
    }

    // -- config --
    /// the name of the limb
    [SerializeField] Name mName;

    // -- nodes --
    /// the input action for this limb
    InputAction mInput;

    /// the shared prompt ui
    Prompt mPrompt;

    // -- lifecycle --
    void Start() {
        // get node deps
        var inputs = GetComponentInParent<GoldPlayerInputSystem>().InputAsset;
        mInput = inputs.FindAction(FindActionName());
        mPrompt = Prompt.Get;
    }

    void OnDisable() {
        mInput.Disable();
    }

    // -- props(hot) --
    /// the limb's transform position
    public Vector3 position {
        get => transform.position;
        set => transform.position = value;
    }

    // -- commands --
    /// show prompt and bind this limb's input
    public void Bind() {
        mPrompt.Set(FindPromptText());
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
            Name.LeftFoot => "hold z w/ ur left pinky",
            Name.RightFoot => "hold the farthest key u can w/ ur thumb",
            Name.LeftHand => "hold the farthest key u can w/ ur ring finger",
            Name.RightHand => "hold the farthest key u can w/ ur index finger",
            _ => null
        };
    }
}
